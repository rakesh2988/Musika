import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { User } from '../../../models/usermodel';
import { HeaderService } from '../../../header/nav-menu.service';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'stafflisting-component',
  templateUrl: './stafflisting.component.html',
  styleUrls: ['./stafflisting.component.css']
})

export class StaffListingComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  userID: number = 0;
  currentUser: User;
  users: User[] = [];
  msgcss: string = '';
  IsMessage: boolean = false;
  responseMessage: string = '';
  public eventList: User[];
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
    this.GetAllStaff();
  }

  GetAllStaff() {
    this.spinner.show();
    this.RvCellService.getStaffList(this.currentUser.UserID.toString())
       .subscribe(data => {
         this.eventList = data.Response.Message;
         this.spinner.hide();
        },
        error => {
          this.spinner.hide();
        });
  }

 

  delete(eventId) {
    var ans = confirm("Do you want to delete this Staff Member");
    if (ans) {
      this.spinner.show();
    
      this.RvCellService.deleteStaffMember(eventId).subscribe((data) => {
       
        if (data.ReturnMessage == "Success") {
          this.GetAllStaff();
          this.msgcss = "text-success";
          this.IsMessage = true;
          this.responseMessage = "Staff Deleted Successfully."
          this.spinner.hide();
        }
        else {
          this.msgcss = "text-danger";
          this.IsMessage = true;
          this.responseMessage = data.ReturnMessage;
          this.spinner.hide();
        }
        setTimeout(() => {
          this.IsMessage = false;        }, 8000);
        
      }, error => console.error(error))
    }
  }
}
