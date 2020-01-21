import { Component, OnInit, Input, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
//import { RVCellService } from '../../../service/service';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { IMyDpOptions, IMyDate, IMyDateModel, IMyOptions } from 'mydatepicker';
import { MyDatePickerModule } from 'mydatepicker';
import { RVCellService } from '../../../service/service';
import { HeaderService } from '../../../header/nav-menu.service';
import { User } from '../../../models/usermodel';
import { NgxSpinnerService } from 'ngx-spinner';

//var generator = require('generate-password');
@Component({
  selector: 'coupon-component',
  templateUrl: './coupon.component.html',
  providers: [RVCellService],
  styleUrls: ['./coupon.component.css']
})

export class CouponComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  title: string = "Create";
  currentUser: User;
  errorMessage: any;
  registerForm: FormGroup;
  msgcss: string = '';
  IsMessage: boolean = false;
  IsShowPassword: boolean = false;
  responseMessage: string = '';
  UserID: number;
  ID: number;
  eventname: string = '';

  Eventlist: Array<string> = [];
  categorylist: Array<string> = [];
  today = new Date();

  textevent: string = '';
 
  public startDate: IMyOptions = {
    // start date options here...
    openSelectorOnInputClick: true,
    inline: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate()-1 }
  };
  public arrDate: IMyDate = { year: 0, month: 0, day: 0 };
  public arrDT: string = '';
  constructor(private _fb: FormBuilder, private route: ActivatedRoute, private router: Router, private spinner: NgxSpinnerService,
    private RvCellService: RVCellService, public nav: HeaderService
  ) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    nav.IsAuthentication();

    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
    

    else {
      if (this.route.snapshot.params["id"]) {
        this.ID = this.route.snapshot.params["id"];
      }
      this.registerForm = this._fb.group({
        Id: 0,
        EventName: ['', [Validators.required]],
        CouponCode: ['', [Validators.required]],
        Discount: ['', [Validators.required]],
        ExpiryDate: ['', [Validators.required]],
        TicketCategory: ['',[Validators.required]],
        Status: [''],
        CreateOn: [''],
        MessageResponse: [''],
        EventID: [''],
        TicketCategoryID: [''],
        CreatedBy: [this.currentUser.UserID]
      });
    }
  }
  GetAllEvents() {
    this.spinner.show();
    this.RvCellService.eventlisting(this.currentUser.UserID.toString())
      .subscribe(data => {
        if (data != "") {
          this.Eventlist = data;
        }
        this.spinner.hide();
      },
        error => {
          //this.loading = false;
        });
  }
  ngOnInit() {
    this.GetAllEvents();
    if (this.ID > 0) {

      this.title = "Edit";
      this.spinner.show();
      this.RvCellService.getCouponById(this.ID.toString())
        .subscribe((resp) => {
          var ExpiryDate = resp[0].ExpiryDate.split("T");
          resp[0].ExpiryDate = ExpiryDate[0];
          this.arrDate = ExpiryDate[0];
          this.eventname = resp[0].EventName;
          this.RvCellService.getCategoryByEventId(resp[0].EventID)
            .subscribe((data) => {
              this.categorylist = data;
              this.registerForm.setValue(resp[0]);
              this.registerForm.controls['EventName'].setValue(resp[0].EventID);
              this.registerForm.controls['TicketCategory'].setValue(resp[0].TicketCategory);
              this.spinner.hide();
            }, error => this.errorMessage = error)
          
        }, error => this.errorMessage = error);
    }


  }
  onchange(value) {

    this.RvCellService.getCategoryByEventId(value.target.value)
      .subscribe((data) => {
       
        this.textevent = value.target.selectedOptions[0].innerText;
        this.eventname = value.target.selectedOptions[0].innerText;
        this.categorylist = data;

      }, error => this.errorMessage = error)
  }
  public doSelect = (value: any) => {
    
    this.RvCellService.getCategoryByEventId(value)
      .subscribe((data) => {
        this.categorylist = data;

      }, error => this.errorMessage = error)
  }
  onArrDateChanged(event: IMyDateModel) {
    // Update value of selDate variable
    this.arrDate = event.date;
    this.arrDT = event.formatted;
  }

  register() {
    
    if (!this.registerForm.valid) { return; }
    this.spinner.show();
    if (this.title == "Create") {

      this.title = "Create";

      this.registerForm.controls['EventName'].setValue(this.textevent);
      this.registerForm.controls['ExpiryDate'].setValue(this.arrDT);
      this.registerForm.controls['CreatedBy'].setValue(this.currentUser.UserID.toString());
      this.RvCellService.SaveCoupon(this.registerForm.value)
        .subscribe((data) => {

          if (data.json.Status) {
            this.msgcss = "text-success";
            this.IsMessage = true;
            this.responseMessage = "Coupon Added Successfully."
            this.router.navigate(['/couponlisting']);
          }
          else {
            this.msgcss = "text-danger";
            this.IsMessage = true;
            this.responseMessage = data.json.RetMessage;
          }
          this.spinner.hide();
        }, error => {
          this.spinner.hide();
        });
    }
    else if (this.title == "Edit") {
      this.spinner.show();
      this.registerForm.controls['EventName'].setValue(this.eventname);
      if ((typeof (this.arrDate) == "object") && (this.arrDT != "")) {
        this.registerForm.controls['ExpiryDate'].setValue(this.arrDT);
      }
      else {
        this.registerForm.controls['ExpiryDate'].setValue(this.arrDate);
      }
      
      this.registerForm.controls['CreatedBy'].setValue(this.currentUser.UserID.toString());
      this.RvCellService.UpdateCoupon(this.registerForm.value)
        .subscribe((data) => {
          
          if (data.json.Status) {
            this.msgcss = "text-success";
            this.IsMessage = true;
            this.responseMessage = "Coupon Updated Successfully."
            this.spinner.hide();
            this.router.navigate(['/couponlisting']);
          }
          else {
            this.msgcss = "text-danger";
            this.IsMessage = true;
            this.responseMessage = data.json.RetMessage;
            this.spinner.hide();
          }

        },
          error => {
            this.spinner.hide();
            //this.loading = false;
          });
    }
  }

  get CouponCode() { return this.registerForm.get('CouponCode'); }
  get EventName() { return this.registerForm.get('EventName'); }
  get TicketCategory() { return this.registerForm.get('TicketCategory'); }
  get ExpiryDate() { return this.registerForm.get('ExpiryDate'); }
  get Discount() { return this.registerForm.get('Discount');  }
}
