import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TokenResponse } from '../../models/TokenResponse';
import { DownloadFileRequest } from '../../models/DownloadFileRequest';
import { FileMetaDataResponse } from '../../models/FileMetaDataResponse';

@Injectable({
  providedIn: 'root'
})
export class FileService {

  private readonly apiUrl = "/api/files";

  constructor(private httpClient: HttpClient) { }
   
  getAll(): Observable<HttpResponse<FileMetaDataResponse[]>> {
     return this.httpClient.get<FileMetaDataResponse[]>(this.apiUrl, { observe: 'response' });
  }
  
  upload(formData: FormData): Observable<HttpResponse<TokenResponse>> {
    return this.httpClient.post<TokenResponse>(this.apiUrl, formData, { observe: 'response' })     
  }

  downloadFile(token: string, request: DownloadFileRequest): Observable<Blob> {
    const url = this.apiUrl + `/download/${token}`;
    return this.httpClient.post(url, request, { responseType: 'blob' });
  }

  findByToken(token: string): Observable<FileMetaDataResponse> {
     return this.httpClient.get<FileMetaDataResponse>(this.apiUrl + "/" + token);
  }

  delete(token: string): Observable<HttpResponse<void>> {
      return this.httpClient.delete<void>(this.apiUrl + "/" + token, { observe: 'response' });
  }
}