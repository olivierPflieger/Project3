import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from "@angular/router";
import { LoginService } from '../../core/service/login/login.service';
import { Login } from '../../core/models/Login';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-login.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})

export class LoginComponent implements OnInit {  
  private loginService = inject(LoginService);
  private formBuilder = inject(FormBuilder);  
  private destroyRef = inject(DestroyRef);
  loginForm: FormGroup = new FormGroup({});
  submitted: boolean = false;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;
  
  // Spinner
  isLoading: boolean = false;
  private timeout: any;
  
  constructor(private router: Router) {}

  ngOnInit() {
    this.loginForm = this.formBuilder.group(
      {
        email: ['', [Validators.required, Validators.email]],      
        password: ['', Validators.required]
      },
    );
  }

  get form() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;    
    if (this.loginForm.invalid) {
      return;
    }
    const loginUser: Login = {
      email: this.loginForm.get('email')?.value,
      password: this.loginForm.get('password')?.value
    };

    this.startLoading();
    this.loginService.login(loginUser)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.stopLoading();
          const token = res.body?.token.toString();
          if (token) {
            this.loginService.setToken(token);
            this.loginService.setUserName(loginUser.email);
            this.router.navigate(['/']);
          } else {
            alert ('The returned token is empty !');            
          }                    
        },
        error: (err) => {                    
          this.stopLoading();
          if (err.error && err.error.errors) {
            const apiErrors = err.error?.errors;
            this.message = Object.values(apiErrors)
              .flat()
              .join('\n'); 
            
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
    this.loginForm.reset();
  }

  startLoading() {
    // lance un timer de 1s
    this.timeout = setTimeout(() => {
      this.isLoading = true;
    }, 1000);
  }

  stopLoading() {
    this.isLoading = false;
    clearTimeout(this.timeout); 
  }
}
