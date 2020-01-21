import { Component, OnInit } from '@angular/core';
import { HeaderService } from '../../header/nav-menu.service';
import { User } from '../../models/usermodel';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivate } from '@angular/router';
import { RVCellService } from '../../service/service';
//import { BreadcrumbService } from 'ng5-breadcrumb';

@Component({
  selector: 'admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.css']
})
export class AdminLayoutComponent implements CanActivate {
  currentUser: User;
  users: User[] = [];
  errorMessage: any;
  //  constructor(public nav: HeaderService, private router: Router) {

  //    nav.IsAuthentication();
  //    if (!nav.isAuthenticate) {
  //      this.router.navigate(['/']);

  //    }
  //    // this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
  //    this.currentUser = JSON.parse(localStorage.getItem('currentUser')); }


  //}
  constructor(private router: Router, private RvCellService: RVCellService, public nav: HeaderService) {
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
    else {
      this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
      //breadcrumbService.addFriendlyNameForRoute('/eventcreation', 'Create Event');
      //this.RvCellService.checkMerchantAccount(this.currentUser.UserID.toString())
      //  .subscribe((data) => {
      //    if (!data) {
      //      localStorage.setItem('isCredit', "false");/* JSON.stringify(user)*/
      //      this.router.navigate(['/creditcard']);

      //      return false;
      //    }

      //  }, error => this.errorMessage = error)
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

