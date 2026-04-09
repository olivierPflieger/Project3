import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TokenResponse } from '../../models/TokenResponse';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  constructor(private httpClient: HttpClient) { }
    
  upload(file: FormData): Observable<HttpResponse<TokenResponse>> {
    return this.httpClient.post<TokenResponse>('/api/files', file, { observe: 'response' })     
  }
}