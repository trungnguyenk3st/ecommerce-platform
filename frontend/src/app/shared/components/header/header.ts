import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/api.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './header.html'
})
export class HeaderComponent implements OnInit {
  readonly categories = signal<Category[]>([]);
  searchTerm = '';

  constructor(
    readonly auth: AuthService,
    readonly cart: CartService,
    private readonly categoryService: CategoryService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.categoryService.getTree().subscribe(categories => this.categories.set(categories));
  }

  onSearch(): void {
    const term = this.searchTerm.trim();
    this.router.navigate(['/products'], { queryParams: term ? { search: term } : {} });
  }

  logout(): void {
    this.auth.logout();
    this.cart.reset();
    this.router.navigate(['/']);
  }
}
