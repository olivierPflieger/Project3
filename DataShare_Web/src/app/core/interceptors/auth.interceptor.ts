import { HttpEvent, HttpHandlerFn, HttpHeaders, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoginService } from '../service/login/login.service';
import { Router } from "@angular/router";
import { catchError, Observable, throwError } from 'rxjs';

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const loginService = inject(LoginService);
  const token = loginService.getToken();
  const router = inject(Router);

  if (!token) { 
    return next(req)
  }

  const headers = new HttpHeaders({
    Authorization: `Bearer ${token}`
  })

  const newReq = req.clone({
    headers
  })

  return next(newReq).pipe(
    catchError(error => {
      console.log(error)

      if (error.status === 401) {
        loginService.logout() 
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  )   
}