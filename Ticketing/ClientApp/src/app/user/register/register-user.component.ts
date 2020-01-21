import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../service/service';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { debug } from 'util';

@Component({
  selector: 'register-user-component',
  templateUrl: './register-user.component.html',
  providers: [RVCellService],
  styleUrls: ['./register-user.component.css']
})

export class RegisterUserComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  errorMessage: any;
  registerForm: FormGroup;
  msgcss: string = '';
  IsMessage: boolean = false;
  IsPopup: boolean = false;
  responseMessage: string = '';
  constructor(private _fb: FormBuilder, private route: ActivatedRoute, private router: Router,
    private RvCellService: RVCellService
  ) {

    this.registerForm = this._fb.group({
      UserID: 0,
      //UserType: ['Event Organizer'],
      UserName: ['', [Validators.required]],
      Email: ['', Validators.compose([
        Validators.required,
        Validators.pattern('^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$')
      ])],
      Password: ['', [Validators.required]],
     
      CreatedDate: [new Date()],

    });
  }

  ngOnInit() {

    // get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
  closepopup() {
    this.IsPopup = false;
    this.router.navigate(['/']);
  }
  register() {
    if (!this.registerForm.valid) { return; }
    this.loading = true;
    this.RvCellService.registerNewUser(this.registerForm.value)
      .subscribe((data) => {
        if (JSON.parse(data.status).MessageResponse.ResponseId == 200) {
          this.IsPopup = true;
          
        }
        else {
          this.msgcss = "text-danger";
          this.IsMessage = true;
          this.responseMessage = JSON.parse(data.status).MessageResponse.ReturnMessage;
        }
        this.loading = false;
        setTimeout(() => {
          this.IsMessage = false;        }, 16000);
      },
        error => {
          this.loading = false;
        });
  }

  //

  get UserName() { return this.registerForm.get('UserName'); }
  get Email() { return this.registerForm.get('Email'); }
  get Password() { return this.registerForm.get('Password'); }
  
}
