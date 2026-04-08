import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../../models/User';
import { TokenResponse } from '../../models/TokenResponse';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private httpClient: HttpClient) { }
    
  register(user: User): Observable<HttpResponse<TokenResponse>> {
    return this.httpClient.post<TokenResponse>('/api/users', user, { observe: 'response' })     
  }
}