import { Component } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { User } from '../models/usermodel';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../service/service';
import { HeaderService } from '../header/nav-menu.service';
@Component({
  selector: 'contact-component',
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css'],
  providers: [RVCellService]
})

export class ContactComponent {

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
    private RvCellService: RVCellService, public nav: HeaderService, private spinner: NgxSpinnerService) {
    this.registerForm = this._fb.group({
      Id: 0,
      Name: ['', [Validators.required]],
      Email: ['', Validators.compose([
        Validators.required,
        Validators.pattern('^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$')
      ])],

      Address: ['', [Validators.required]],
      Message: ['', [Validators.required]]

    });
  }
  register() {
    
    if (!this.registerForm.valid) { return; }
    this.spinner.show();
    this.RvCellService.registerContactForm(this.registerForm.value)
      .subscribe((data) => {
        if (data.json.ResponseId == 200) {
          this.msgcss = "text-success";
          this.IsMessage = true;
          this.spinner.hide();
          this.responseMessage = "Contact Form submit Successfully."
          //this.router.navigate(['/stafflisting']);
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
  get Name() { return this.registerForm.get('Name'); }
  get Email() { return this.registerForm.get('Email'); }
  
  get Address() { return this.registerForm.get('Address'); }
  get Message() { return this.registerForm.get('Message'); }
 
}
