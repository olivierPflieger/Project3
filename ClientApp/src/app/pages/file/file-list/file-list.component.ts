import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { FileService } from '../../../core/service/file/file.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DownloadFileRequest } from '../../../core/models/DownloadFileRequest';
import { FileMetaDataResponse } from '../../../core/models/FileMetaDataResponse';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-file-list.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './file-list.component.html',
  styleUrl: './file-list.component.css',
})

export class FileListComponent implements OnInit { 
  fileMetaDataResponses: FileMetaDataResponse[] = [];
  downloadDownloadFileRequest: DownloadFileRequest | null = null;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;  
  fileToken : string | null = null;
  private destroyRef = inject(DestroyRef);
  fileForm: FormGroup = new FormGroup({});
  isLoading: boolean = false;
  
  constructor(private router: Router, private route: ActivatedRoute, public fileService: FileService) {}
  
  ngOnInit() {
    this.loadFilesMetaDatas();
  }

  loadFilesMetaDatas() {
    this.fileService.getAll()
    .subscribe({
      next: (res: HttpResponse<FileMetaDataResponse[]>) => {
        this.fileMetaDataResponses = res.body || [];
      },
      error: (err) => {
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
  
}