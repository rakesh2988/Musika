import { Injectable } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { retry } from 'rxjs/operators';

@Injectable()
export class HeaderService {
  isAuthenticate: boolean = false;

  constructor(private route: ActivatedRoute, private router: Router) {
    
    //this.isAuthenticate = false;
   // if (localStorage.getItem('currentUser')) {
    if (localStorage.getItem('currentUser')) {
      // logged in so return true
      this.isAuthenticate = true;
    }
    else {
      this.isAuthenticate = false;
      this.router.navigate(['/']);
    }
    this.IsAuthentication();
  }

  IsAuthentication() {
    if (localStorage.length != 0) {
      if (localStorage.getItem('currentUser')) {
        return this.isAuthenticate = true;
      }
      else {
        return this.isAuthenticate = false;

      }
    }
    else {
      return this.isAuthenticate = false;
    }
  }
  //IsCreditAvalible() {
  //  if (localStorage.length != 0) {
  //    if (localStorage.getItem('isCredit') == "false") {
        
  //      return false;
  //    }
  //    else {
  //      return true;
  //    }
  //  }
  //}
}
