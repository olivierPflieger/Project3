import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { LoginService } from './core/service/login/login.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'datashare';
  
  constructor(private router: Router, public loginService: LoginService) {}

  ngOnInit() {
    if (this.loginService.isTokenExpired()) {
      this.loginService.logout();
    }    
  }

  logout() {
      this.loginService.logout();      
  }
}
