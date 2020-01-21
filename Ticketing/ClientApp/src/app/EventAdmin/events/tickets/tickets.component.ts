import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { User } from '../../../models/usermodel';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'tickets-component',
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.css']
})

export class TicketsComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  ticketForm: FormGroup;
  userID: number = 0;
  currentUser: User;
  users: User[] = [];
  errorMessage: any;
  public eventList: User[];
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private _fb: FormBuilder,
    private RvCellService: RVCellService
  ) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    this.ticketForm = this._fb.group({
      TicketId: 0,
      EventID: 0,
      TicketNumber: [''],
      UserId:0

    });
  }

  ngOnInit() {

  }

  save() {
    
    //if (!this.registerForm.valid) {
    //  return;
    //}

    this.RvCellService.saveTicketsDetail(this.ticketForm.value)
      .subscribe((data) => {
        this.router.navigate(['/eventlisting']);
      }, error => this.errorMessage = error)
  }

}


