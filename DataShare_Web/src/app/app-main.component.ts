import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { LoginService } from './core/service/login/login.service';
import { SentryExempleComponent } from './sentry-example.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, SentryExempleComponent],
  templateUrl: './app-main.component.html',
  styleUrl: './app-main.component.css'
})
export class AppMainComponent {
  title = 'datashare';
  isMenuHamburgerOpen = false;
  
  constructor(private router: Router, public loginService: LoginService) {}

  ngOnInit() {
    if (this.loginService.isTokenExpired()) {
      this.loginService.logout();
    }    
  }

  logout() {
      this.loginService.logout();      
      this.router.navigate(['/']);     
  }

  toggleMenuHamburger() {
    this.isMenuHamburgerOpen = !this.isMenuHamburgerOpen;
  }

}
