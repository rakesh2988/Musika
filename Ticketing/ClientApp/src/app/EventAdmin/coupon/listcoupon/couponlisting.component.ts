import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { User } from '../../../models/usermodel';
import { HeaderService } from '../../../header/nav-menu.service';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'CouponListing-component',
  templateUrl: './couponlisting.component.html',
  styleUrls: ['./couponlisting.component.css']
})

export class CouponListingComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  userID: number = 0;
  currentUser: User;
  users: User[] = [];
  msgcss: string = '';
  IsMessage: boolean = false;
  responseMessage: string = '';
  public couponList: User[];
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public nav: HeaderService,
    private RvCellService: RVCellService, private spinner: NgxSpinnerService
  ) {
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
   
    else {
      this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    }
  }
  
  ngOnInit() {
    this.GetAllCoupon();

  }

  GetAllCoupon() {
    this.spinner.show();
    this.RvCellService.getcouponlisting(this.currentUser.UserID.toString())
      .subscribe(data => {
        
         this.couponList = data;
         this.spinner.hide();
        },
        error => {
          this.spinner.hide();
        });
  }
  delete(couponId) {
    var ans = confirm("Do you want to delete this Coupon?");
    if (ans) {
      this.spinner.show();
      this.RvCellService.deleteCoupon(couponId).subscribe((data) => {
        if (data.ReturnMessage == "Success") {
         
          this.msgcss = "text-success";
          this.IsMessage = true;
          this.responseMessage = "Coupon Deleted Successfully."
          this.spinner.hide();
        }
        else {
          this.msgcss = "text-danger";
          this.IsMessage = true;
          this.responseMessage = data.ReturnMessage;
          this.spinner.hide();
        }
        this.GetAllCoupon();
        setTimeout(() => {
          this.IsMessage = false;        }, 8000);

      }, error => console.error(error))
    }
  }
 
}
