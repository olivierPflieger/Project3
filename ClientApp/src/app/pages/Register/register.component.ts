import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from "@angular/router";
import { UserService } from '../../core/service/user/user.service';
import { User } from '../../core/models/User';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { passwordMatchValidator } from '../../core/validators/password-match.validator';

@Component({
  selector: 'app-register.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})

export class RegisterComponent implements OnInit {  
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);
  userForm: FormGroup = new FormGroup({});
  submitted: boolean = false;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;

  constructor(private router: Router) {}

  ngOnInit() {
    this.userForm = this.formBuilder.group(
      {        
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', Validators.required]
      },
      {
        validators: passwordMatchValidator
      }
    );
  }

  get form() {
    return this.userForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.userForm.invalid) {
      return;
    }
    const newUser: User = {
      email: this.userForm.get('email')?.value,
      password: this.userForm.get('password')?.value
    };    
    
    this.userService.register(newUser)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.message = 'Inscription réussie !';
          this.messageType = 'success';
        },
        error: (err) => {                    
          if (err.error && err.error.errors) {
            const apiErrors = err.error?.errors;
            this.message = Object.values(apiErrors)
              .flat()
              .join('\n'); 

            console.log("erreur: " + this.message);
            
          } else {
            if (err.error && err.error.message) {
              this.message = err.error.message;
            } else {
              this.message = err.status + ' - ' + err.statusText;
            }
          }
          this.messageType = 'error';
        }
      });
  }

  onReset(): void {
    this.submitted = false;
    this.userForm.reset();
  }
}
