import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from "@angular/router";
import { FileService } from '../../../core/service/file/file.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl } from '@angular/forms';
import { FileMetaData } from '../../../core/models/FileMetaData';

@Component({
  selector: 'app-file-form.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './file-form.component.html',
  styleUrl: './file-form.component.css',
})

export class FileFormComponent implements OnInit { 
  private fileService = inject(FileService);
  private formBuilder = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);
  fileForm: FormGroup = new FormGroup({});
  submitted: boolean = false;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;
  selectedFile!: File;
  showForm: boolean = true;
  fileMetaData: FileMetaData | null = null;

  expirations = [
    { value: '1', viewValue: '1 journée' },
    { value: '2', viewValue: '2 journées' },
    { value: '3', viewValue: '3 journées' },
    { value: '4', viewValue: '4 journées' },
    { value: '5', viewValue: '5 journées' },
    { value: '6', viewValue: '6 journées' },
    { value: '7', viewValue: '7 journées' }
  ];
  selectExpiration = new FormControl<string>('7', { nonNullable: true });

  tagInput = new FormControl<string>('', { nonNullable: true });

  tags: string[] = [];

  constructor(private router: Router) {}

  ngOnInit() {
    this.fileForm = this.formBuilder.group(
      {        
        password: ['', Validators.minLength(6)]
      }      
    );
  }

  get form() {
    return this.fileForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.fileForm.invalid) {
      return;
    }

    const formData = new FormData();
    formData.append('password', this.fileForm.get('password')?.value);
    formData.append('tags', this.tags.join(','));
    formData.append('expiration', this.selectExpiration.value);
    formData.append('file', this.selectedFile);

    this.fileService.upload(formData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.showForm = false;
          this.fileMetaData = response.body as FileMetaData;
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
              if (err.error) {
                this.message += ': ' + err.error;
              }
            }
          }
          this.messageType = 'error';
        }
      });
  }

  onReset(): void {
    this.submitted = false;
    this.fileForm.reset();
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onKeyUp(event: KeyboardEvent) {
    const value = this.tagInput.value;

    if (value.includes(',')) {
      const parts = value.split(',');

      parts.forEach(part => this.addTag(part));

      this.tagInput.setValue('');
    }
  }

  addTag(rawTag: string) {
    const tag = rawTag.trim();
    
    if (!tag) return;

    if (tag.length > 30) return;

    const exists = this.tags.some(t => t.toLowerCase() === tag.toLowerCase());
    if (exists) return;

    this.tags.push(tag);
  }

  removeTag(index: number) {
    this.tags.splice(index, 1);
  }
}
