import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { FileService } from '../../../core/service/file/file.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FileMetaData } from '../../../core/models/FileMetaData';
import { DownloadFileRequest } from '../../../core/models/DownloadFileRequest';

@Component({
  selector: 'app-file-details.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './file-details.component.html',
  styleUrl: './file-details.component.css',
})

export class FileDetailsComponent implements OnInit { 
  fileMetaData: FileMetaData | null = null;
  downloadDownloadFileRequest: DownloadFileRequest | null = null;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;  
  fileToken : string | null = null;
  private destroyRef = inject(DestroyRef);
  fileForm: FormGroup = new FormGroup({});
  isLoading: boolean = false;
  
  constructor(private router: Router, private route: ActivatedRoute, public fileService: FileService) {}
  
  ngOnInit() {
    
    this.fileToken = String(this.route.snapshot.paramMap.get('id'));
        
    this.fileService.findByToken(this.fileToken)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (fileMetaData) => {
          this.messageType = 'success';
          this.fileMetaData = fileMetaData;
        },
        error: (err) => {
          if (err.error && err.error.message) {
            this.message = err.error.message;
            this.messageType = 'error';
          } else {
            this.message = err.status + ' - ' + err.statusText;
            this.messageType = 'error';
          }
        }
      });

      this.fileForm = new FormGroup({
        password: new FormControl<string>('', { nonNullable: true })
      });
  }

  downloadFile() {    
    const token = this.fileMetaData?.token || '';
    this.isLoading = true;
    this.message = "";
    this.messageType = 'success';
    this.fileService.downloadFile(token, this.fileForm.value).subscribe({
      next: (blobInfo: Blob) => {
        
        const downloadUrl = window.URL.createObjectURL(blobInfo);
        
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = this.fileMetaData?.originalFileName || 'downloaded_file';
        
        // Simuler le clic
        document.body.appendChild(link); // Requis pour certains navigateurs anciens
        link.click();
        
        // Nettoyer la mémoire et l'élément
        link.remove();
        window.URL.revokeObjectURL(downloadUrl);

        this.isLoading = false;
        this.message = "Fichier correctement téléchargé!";
        this.messageType = 'success';
      },
      error: (err) => {        
        
        // Récupération du message d'erreur envoyé par le backend (puisqu'on récupère un blob)
        err.error.text().then((text: string) => {
          this.isLoading = false;
          try {          
            const errorObj = JSON.parse(text);          
            this.message = errorObj.message;              
          } catch (e) {
            this.message = "Erreur inattendue, impossible de lire le JSON";
          }
        })                
      }
    });
  }
}