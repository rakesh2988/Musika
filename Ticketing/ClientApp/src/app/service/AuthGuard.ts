import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { RVCellService } from './service';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
      if (localStorage.getItem('currentUser')) {
            // logged in so return true
            return true;
        }

        // not logged in so redirect to login page with the return url
        this.router.navigate(['/login'], { queryParams: { returnUrl: state.url }});
        return false;
  }
}

//@Injectable()
//export class AuthGuard implements CanActivate {
//  constructor(
//    private _fooAuth: FooAuth,
//    private RvCellService: RVCellService,
//    private _router: Router
//  ) { }
//  canActivate(): Observable<boolean> {
//    return this.isAllowedAccess();
//  }
//  private isAllowedAccess() {
//    if (!this._fooAuth.currentSession) {
//      this._router.navigate(['/login']);
//      return Observable.of(false);
//    }
//    return Observable
//      .fromPromise(this.RvCellService.validateSession(this.RvCellService.currentSession))
//      .mapTo(true)
//      .catch(err => {
//        this._router.navigate(['/login']);
//        return Observable.of(false)
//      });
//  }
