import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { debug } from 'util';
import { HeaderService } from '../../../header/nav-menu.service';
import { User } from '../../../models/usermodel';
import { NgxSpinnerService } from 'ngx-spinner';
//var generator = require('generate-password');
@Component({
  selector: 'staffcreation-component',
  templateUrl: './staffcreation.component.html',
  styleUrls: ['./staffcreation.component.css'],
  providers: [RVCellService]
})

export class StaffCreationComponent implements OnInit {
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
  constructor(private _fb: FormBuilder, private route: ActivatedRoute, private router: Router,
    private RvCellService: RVCellService, public nav: HeaderService, private spinner: NgxSpinnerService
  ) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
   
    else {
      if (this.route.snapshot.params["id"]) {
        this.UserID = this.route.snapshot.params["id"];
      }
      this.registerForm = this._fb.group({
        UserID: 0,
        UserName: ['', [Validators.required]],
        Email: ['', Validators.compose([
          Validators.required,
          Validators.pattern('^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$')
        ])],
         Password: [''],
        Addres: ['', [Validators.required]],
        City: ['', [Validators.required]],
        State: ['', [Validators.required]],
        Country: ['', [Validators.required]],
         PostalCode: ['', [Validators.required]],
        PhoneNumber: ['', Validators.compose([Validators.pattern("^[0-9]*$") ])],
        UserType: ['Staff'],
        //CreatedDate: [new Date()],
        CreatedBy: [this.currentUser.UserID],
      });
    }
  }

  ngOnInit() {
    if (this.UserID > 0) {
      this.spinner.show();
      this.title = "Edit";
      this.IsShowPassword = true;
      this.RvCellService.getStaffById(this.UserID.toString())
        .subscribe((resp) => {
          this.registerForm.controls['CreatedBy'].setValue(this.currentUser.UserID.toString());
        //  this.registerForm.controls['CreatedDate'].setValue(new Date());
          this.registerForm.setValue(resp);
          this.spinner.hide();
        }, error => {
          this.spinner.hide();
          this.errorMessage = error
        });
    }
  }
  
  register() {
    
    if (!this.registerForm.valid) { return; }
    this.spinner.show();
    if (this.title == "Create") {
     this.IsShowPassword = false;
      this.registerForm.controls['CreatedBy'].setValue(this.currentUser.UserID.toString());
      this.RvCellService.registerNewUser(this.registerForm.value)
        .subscribe((data) => {
          if (JSON.parse(data.status).MessageResponse.ResponseId == 200) {
            this.msgcss = "text-success";
            this.IsMessage = true;
            this.spinner.hide();
            this.responseMessage = "Staff Register Successfully."
            this.router.navigate(['/stafflisting']);
          }
          else {
            this.msgcss = "text-danger";
            this.IsMessage = true;
            this.responseMessage = JSON.parse(data.status).MessageResponse.ReturnMessage;
            this.spinner.hide();
          }
        }, error => {
          this.spinner.hide();
          });
    }
    else if (this.title == "Edit") {
      this.spinner.show();

      this.RvCellService.updateStaffmember(this.registerForm.value)
        .subscribe((data) => {
        
          if (data.json.ReturnMessage == "Success") {
            this.msgcss = "text-success";
            this.IsMessage = true;
            this.responseMessage = "Staff Updated Successfully."
            this.spinner.hide();
            this.router.navigate(['/stafflisting']);
          }
          else {
            this.msgcss = "text-danger";
            this.IsMessage = true;
            this.responseMessage = data.json.ReturnMessage;
            this.spinner.hide();
          }
          
        },
        error => {
          this.spinner.hide();
            //this.loading = false;
          });
    }
  }

  get UserName() { return this.registerForm.get('UserName'); }
  get Email() { return this.registerForm.get('Email'); }
  //get Password() { return this.registerForm.get('Password'); }
  get Addres() { return this.registerForm.get('Addres'); }
  get City() { return this.registerForm.get('City'); }
  get State() { return this.registerForm.get('State'); }
  get Country() { return this.registerForm.get('Country'); }
  get PhoneNumber() { return this.registerForm.get('PhoneNumber'); }
  get PostalCode() { return this.registerForm.get('PostalCode'); }
}
