import { Component, OnInit } from '@angular/core';
import { HeaderService } from '../../header/nav-menu.service';
import { User } from '../../models/usermodel';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivate } from '@angular/router';
import { RVCellService } from '../../service/service';
//import { BreadcrumbService } from 'ng5-breadcrumb';

@Component({
  selector: 'user-layout',
  templateUrl: './user-layout.component.html',
  styleUrls: ['./user-layout.component.css']
})
export class UserLayoutComponent implements CanActivate {
  currentUser: User;
  users: User[] = [];
  errorMessage: any;
  
  constructor(private router: Router, private RvCellService: RVCellService, public nav: HeaderService) {
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
    else {
      this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
      
    }

  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (localStorage.getItem('currentUser')) {
      // logged in so return true
      return true;
    }

    // not logged in so redirect to login page with the return url
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}

