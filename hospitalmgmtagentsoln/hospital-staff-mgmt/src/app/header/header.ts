import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: './header.html',
  imports: [CommonModule,FormsModule ],
})
export class Header {
  constructor(private router: Router) {}

  onLogout() {
    localStorage.clear(); // or remove token
    this.router.navigate(['/login']);
  }
}
