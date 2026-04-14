import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Login } from '../../models/Login';
import { TokenResponse } from '../../models/TokenResponse';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  constructor(private httpClient: HttpClient) { }
  
  private readonly ID_TOKEN = 'id_token';
  private readonly USERNAME = 'userName';

  login(loginUser: Login): Observable<HttpResponse<TokenResponse>> {
    return this.httpClient.post<TokenResponse>('/api/login', loginUser, { observe: 'response' })     
  }

  public setToken(token: string): void {
    localStorage.setItem(this.ID_TOKEN, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.ID_TOKEN);
  }

  public setUserName(userName: string): void {
    localStorage.setItem(this.USERNAME, userName);
  }

  getUserName(): string | null {
    return localStorage.getItem(this.USERNAME);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false
    } else { return true};
  }

  logout() {
    localStorage.removeItem(this.ID_TOKEN);
    localStorage.removeItem(this.USERNAME);
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    const payload = JSON.parse(atob(token.split('.')[1]));
    const exp = payload.exp;

    const now = Math.floor(Date.now() / 1000);

    return exp < now;
  }
}