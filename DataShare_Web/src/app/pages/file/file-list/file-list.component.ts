import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { FileService } from '../../../core/service/file/file.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DownloadFileRequest } from '../../../core/models/DownloadFileRequest';
import { FileMetaDataResponse } from '../../../core/models/FileMetaDataResponse';
import { HttpResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { LoginService } from '../../../core/service/login/login.service';

@Component({
  selector: 'app-file-list.component',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, FormsModule],
  standalone: true,
  templateUrl: './file-list.component.html',
  styleUrl: './file-list.component.css',
})

export class FileListComponent implements OnInit { 
  fileMetaDatas: FileMetaDataResponse[] = [];
  filteredFiles: any[] = [];  
  tagFilter: string = '';
  downloadDownloadFileRequest: DownloadFileRequest | null = null;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;  
  fileToken : string | null = null;
  private destroyRef = inject(DestroyRef);
  fileForm: FormGroup = new FormGroup({});  
  domain: string = '';
  filter: string = 'valid';
  isMenuActionsOpen: boolean = false;
  openedMenuToken: string | null = null;
  isSidebarVisible = false;
  
  constructor(private router: Router, private route: ActivatedRoute, public fileService: FileService, public loginService: LoginService) {
    this.domain = window.location.origin;
  }
  
  ngOnInit() {
    this.loadFilesMetaDatas();
  }

  loadFilesMetaDatas() {    
    this.fileService.getAll()
    .subscribe({
      next: (res: HttpResponse<FileMetaDataResponse[]>) => {        
        this.fileMetaDatas = res.body || [];
        this.filterFiles();
      },
      error: (err) => {        
        if (err && err.status === 404) {
          this.fileMetaDatas = [];
          this.filteredFiles = [];
        }
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
  
  deleteFile(token: string, event: Event) {    
    event.preventDefault();
    const confirmed = window.confirm('Etes vous sur de vouloir supprimer ce fichier?');

    if (confirmed) {
      
      this.fileService.delete(token)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {                      
            this.message = "Fichier correctement supprimé";
            this.messageType = 'success';
            this.closeMenuActionsMobile();
            // List refresh
            this.loadFilesMetaDatas();
          },
          error: (err) => {            
            if (err.error && err.error.message) {
                this.message = err.statusText + ': ' + err.error.message;
              } else {
                this.message = err.statusText + ': ' + err.error;
              }
              this.messageType = 'error';            
          }
        });
    }
  }
  
  filterFiles() {
    if (this.filter === 'expired') {
      this.filteredFiles = this.fileMetaDatas.filter(f => f.isExpired);
    } else if (this.filter === 'valid') {
      this.filteredFiles = this.fileMetaDatas.filter(f => !f.isExpired);
    } else {
      this.filteredFiles = this.fileMetaDatas;
    }

    if (this.tagFilter) {
      this.filteredFiles = this.filteredFiles.filter(f =>
        f.tags?.some((tag: string) =>
          tag.toLowerCase().includes(this.tagFilter.toLowerCase())
        )
      );
    }
  }

  logout() {
      this.loginService.logout();      
      this.router.navigate(['/']);     
  }

  getExpirationLabel(isExpired: boolean, remainingDays: number): string {
    
    if (isExpired)      
      return 'Expiré';
    
    if (remainingDays === 0 && !isExpired) return 'Expire aujourd\'hui'; 
    if (remainingDays === 1) return 'Expire demain';
    if (remainingDays === 7) return 'Expire dans 1 semaine';

    return `Expire dans ${remainingDays} jours`;
  }
  
  getExpirationClass(isExpired: boolean, remainingDays: number): string {    
    
    if (isExpired)
      return 'expiration-text-danger';
    
    if (remainingDays === 0 || remainingDays === 1) {
        return 'expiration-text-warning';      
    } else {
      return 'expiration-text-normal';
    }
  }

  toggleMenuActionsMobile(token: string): void {
    this.openedMenuToken = this.openedMenuToken === token ? null : token;
  }

  closeMenuActionsMobile(): void {
    this.openedMenuToken = null;
  }

  toggleSidebar(): void {
    this.isSidebarVisible = !this.isSidebarVisible;
  }
  
}