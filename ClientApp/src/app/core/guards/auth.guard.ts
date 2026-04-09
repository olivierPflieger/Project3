import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { LoginService } from "../service/login/login.service";

export const AuthGuard = () => {
    const auth = inject(LoginService);
    const router = inject(Router);

    if(!auth.isAuthenticated()) {
        router.navigate(['/login']);
        return false
    }
    return true
}