import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.html',
  imports: [CommonModule,FormsModule ],
})
export class Header {
  constructor(private router: Router,private auth:AuthService) {}

  onLogout() {
        this.auth.logout().subscribe({
      next: (res:any) => {
        localStorage.clear(); // or remove token
        this.router.navigate(['/login']);
      },
      error: () => {
        alert('Invalid username or password');
      },
    }); 

  
  }
}
