import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
//import { RVCellService } from '../../../service/service';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { HeaderService } from '../../header/nav-menu.service';
import { User } from '../../models/usermodel';
import { RVCellService } from '../../service/service';
//var generator = require('generate-password');
@Component({
  selector: 'credit-component',
  templateUrl: './credit.component.html',
  providers: [RVCellService],
  styleUrls: ['./credit.component.css']
})

export class CreditCardComponent implements OnInit {
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
  IsCreditMessage: boolean = false;
  constructor(private _fb: FormBuilder, private route: ActivatedRoute, private router: Router,
    private RvCellService: RVCellService, public nav: HeaderService
  ) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);

    }
    
    else {
      //if (this.route.snapshot.params["id"]) {
      //  this.UserID = this.route.snapshot.params["id"];
      //}
      this.registerForm = this._fb.group({
        Id : 0,
        Auth1: ['', [Validators.required]],
        Auth2: ['', [Validators.required]],
        MerchantId: ['', [Validators.required]],
        UserId: [this.currentUser.UserID]
      });
    }
  }

  ngOnInit() {
    
    
  }

  register() {
    if (!this.registerForm.valid) { return; }
    this.loading = true;
    this.RvCellService.registerCreditCard(this.registerForm.value)
      .subscribe((data) => {
        if (data.json.ResponseId == 200) {
          this.msgcss = "text-success";
          this.IsMessage = true;
          this.IsCreditMessage = false;
          localStorage.setItem('isCredit', "true");
          this.responseMessage = "Card detail Register Successfully."
          //this.router.navigate(['/stafflisting']);
        }
        else {
          this.msgcss = "text-danger";
          this.IsMessage = true;
          this.responseMessage = data.json.ReturnMessage;
        }
        this.loading = false;
      }, error => {
        this.loading = false;
      });

  }

  get MerchantId() { return this.registerForm.get('MerchantId'); }
  get Auth2() { return this.registerForm.get('Auth2'); }
  get Auth1() { return this.registerForm.get('Auth1'); }
  
}
