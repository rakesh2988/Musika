import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './Users/register/register.component';
import { ForgetPasswordComponent } from './Users/forgetpassword/forgetpassword.component';
import { ChangePasswordComponent } from './Users/changepassword/changepassword.component';
import { EventCreationComponent } from './EventAdmin/events/eventcreation/eventcreation.component';
//import { EventDetailComponent } from './EventAdmin/events/eventdetail/eventdetail.component';
import { EventListingComponent } from './EventAdmin/events/eventlisting/eventlisting.component';
import { HeaderComponent } from './header/header.component';
import { TicketGenerationComponent } from './ticketgeneration/ticketgeneration.component';
import { FooterComponent } from './footer/footer.component';
import { AdminLayoutComponent } from './_layout/admin-layout/admin-layout.component';
import { TicketsComponent } from './EventAdmin/events/tickets/tickets.component';
import { TicketsAnalyticsComponent } from './EventAdmin/events/ticketsAnalytics/ticketsAnalytics.component';
import { StaffCreationComponent } from './EventAdmin/staff/staffcreation/staffcreation.component';
import { StaffListingComponent } from './EventAdmin/staff/stafflisting/stafflisting.component';
import { CreditCardComponent } from './EventAdmin/credit/credit.component';
import { ContactComponent } from './contact/contact.component';
import { AboutComponent } from './about/about.component';
import { PrivacyComponent } from './privacy/privacy.component';
import { TermsComponent } from './terms/terms.component';
import { CouponComponent } from './EventAdmin/coupon/create/coupon.component';
import { CouponListingComponent } from './EventAdmin/coupon/listcoupon/couponlisting.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  //  { path: 'dashboard', component: DashboardComponent },
  { path: 'forgetpassword', component: ForgetPasswordComponent },
  { path: 'changepassword', component: ChangePasswordComponent },
  { path: 'contact', component: ContactComponent },
  { path: 'about', component: AboutComponent },
  { path: 'privacy', component: PrivacyComponent },
  { path: 'terms', component: TermsComponent },
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      //{ path: 'dashboard', component: DashboardComponent, pathMatch: 'full' },
      {
        path: 'eventcreation', component: EventCreationComponent, data: {
          breadcrumb: "Create Event"
        }
      },
      {
        path: 'event/edit/:id', component: EventCreationComponent, data: {
          breadcrumb: "Edit Event"
        }
      },
      //{
      //  path: 'event/detail/:id', component: EventDetailComponent, data: {
      //    breadcrumb: "Event detail"
      //  }
      //},
      { path: 'tickets', component: TicketsComponent },
      {
        path: 'eventlisting', component: EventListingComponent, data: {
          breadcrumb: "Event Listing"
        }
      },
      {
        path: 'ticketAnalytics/:id', component: TicketsAnalyticsComponent, data: {
          breadcrumb: "Analytics"
        }
      },
      {
        path: 'staffcreation', component: StaffCreationComponent, data: {
          breadcrumb: "Create Staff"
        }
      },
      {
        path: 'staff/edit/:id', component: StaffCreationComponent, data: {
          breadcrumb: "Edit Staff"
        }
      },
      {
        path: 'stafflisting', component: StaffListingComponent, data: {
          breadcrumb: "Staff Listing"
        }
      },
      //{
      //  path: 'eventstafflisting/:id', component: EventStaffListingComponent, data: {
      //    breadcrumb: "Event Staff"
      //  }
      //},
      {
        path: 'creditcard', component: CreditCardComponent, data: {
          breadcrumb: "Credit Card"
        }
      },
      {
        path: 'Couponcreation', component: CouponComponent, data: {
          breadcrumb: "Create Coupon"
        }
      },
      {
        path: 'coupon/edit/:id', component: CouponComponent, data: {
          breadcrumb: "Edit Coupon"
        }
      },
      {
        path: 'couponlisting', component: CouponListingComponent, data: {
          breadcrumb: "Coupon Listing"
        }
      }
    ]
  },
  { path: 'header', component: HeaderComponent },
  { path: 'footer', component: FooterComponent },
  { path: 'ticketgeneration', component: TicketGenerationComponent }
];

export const routingModule = RouterModule.forRoot(routes);
