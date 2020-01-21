import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../service/service';

@Component({
  selector: 'userticketingeventlisting-component',
  templateUrl: './userticketingeventlisting.component.html'
})

export class UserTicketingEventListingComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private RvCellService: RVCellService
    //private authenticationService: AuthenticationService,
    //private alertService: AlertService
  ) { }

  ngOnInit() {
    // reset login status
    //this.authenticationService.logout();

    // get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  userticketingeventlisting() {
    this.loading = true;

    this.RvCellService.userticketingeventlisting(this.model)
       .subscribe(
        data => {
          this.router.navigate([this.returnUrl]);
        },
        error => {
          //this.alertService.error(error);
          this.loading = false;
        });

    //this.authenticationService.login(this.model.username, this.model.password)
    //  .subscribe(
    //    data => {
    //      this.router.navigate([this.returnUrl]);
    //    },
    //    error => {
    //      this.alertService.error(error);
    //      this.loading = false;
    //    });
  }
}
