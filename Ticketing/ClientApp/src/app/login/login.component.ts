import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../service/service';
import { HeaderService } from '../header/nav-menu.service';
import { debug } from 'util';
import { User } from '../models/usermodel';


@Component({
  selector: 'login-component',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})

export class LoginComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  currentUser: User;
  users: User[] = [];
  msgcss: string = '';
  IsMessage: boolean = false;
  responseMessage: string = '';
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private RvCellService: RVCellService, public nav: HeaderService) { nav.IsAuthentication(); }

  ngOnInit() {
    // reset login status
    this.RvCellService.logout();
  }

  login() {
    this.loading = true;
    this.RvCellService.AutheticateUser(this.model)
      .subscribe((data) => {
        if (JSON.parse(data.status).MessageResponse.ResponseId == 200) {
          if (JSON.parse(data.status).UserType == "Event Organizer") {
            this.router.navigate(['/eventlisting']);
          }
        }
        else {
          this.msgcss = "text-danger";
          this.IsMessage = true;
          this.loading = false;
          this.responseMessage = JSON.parse(data.status).MessageResponse.ReturnMessage;
        }

        setTimeout(() => {
          //this.IsMessage = false;        }, 8000);
      }, error => {
        this.loading = false;
      });

  }


}


