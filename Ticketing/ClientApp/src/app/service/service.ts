import { Injectable, Inject } from '@angular/core';
import { Http, Headers, Response } from '@angular/http';
import { RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw';
import { HttpHeaders } from '@angular/common/http';
import { debug } from 'util';


@Injectable()
export class RVCellService {
  myAppUrl: string = "";
  headers: Headers;
  options: RequestOptions;

  // constructor(private _http: Http, @Inject('BASE_URL') baseUrl: string) {
  constructor(private _http: Http) {
    //this.myAppUrl = "http://23.111.138.246/";
    this.myAppUrl = "http://localhost:55172/";
    this.headers = new Headers({ 'Content-Type': 'application/json' });
    this.options = new RequestOptions({ headers: this.headers });
  }

  // New User Registration (Final)
  registerNewUser(users) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/RegisterTicketingUser', users, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }

  // Credit Card Registration (Final)
  registerCreditCard(cardDetails) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/SaveCreditCardDetails', cardDetails, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }
  registerContactForm(contactDetails) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/SaveContactDetails', contactDetails, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }


  // Credit Card Registration (Final)
  SaveCoupon(couponDetails) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/AddCoupon', couponDetails, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }
  // Credit Card Registration (Final)
  UpdateCoupon(couponDetails) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/UpdateCoupon', couponDetails, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }

  updateStaffmember(staff) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/UpdateStaffMember', staff, options)
      .map((response: Response) => {
        if (response) {
          return { status: response.text(), json: response.json() }
        }
      }).catch(this.errorHandler)
  }

  // Login Authentication (Final)
  AutheticateUser(users) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/AuthenticateUser', users, options)
      .map((response: Response) => {
        if (response) {
          localStorage.setItem('currentUser', response.text());/* JSON.stringify(user)*/
          return { status: response.text(), json: response.json() }
        }
      })
      .catch(this.errorHandler)
  }

  checkMerchantAccount(userID: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetCardDetailById?ID=' + userID, options)
      .map((response: Response) => {
        if (response) {
          localStorage.setItem('isCredit', response.text());
        }
        //localStorage.setItem('isCredit', "false");/* JSON.stringify(user)*/
        return response.json();
      })
      .catch(this.errorHandler)
  }

  logout() {
    // remove user from local storage to log user out
    // localStorage.removeItem('currentUser');
    localStorage.removeItem('currentUser');
    localStorage.clear();
    //localStorage.clear();
  }
  //New Event Creation (final)
  eventcreation(events) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/UpdateTicketingEvent', events, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  //New Event Creation (final)
  saveTicketsDetail(tickets) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return null;
  }

  eventUpdation(events) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/UpdateTicketingEvent', events, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Forget or Retrieve Password
  forgetpassword(value) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/TicketingAPI/RetreivePassword', value, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Change Password
  changepassword(value) {
    alert(this._http.post(this.myAppUrl + 'api/User/ChangePassword', value));
    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/User/ChangePassword', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Creation
  staffeventcreation(value) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/staffevents/StaffEventCreation', value, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Feature
  staffeventfeatures(value) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/staffevents/StaffEventFeatures', value, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Listing
  staffeventlisting(value) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/staffevents/StaffEventListing', value, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Ticketing Event Creation
  ticketingeventcreation(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/staffevents/StaffEventCreation', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Feature
  ticketingeventfeatures(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/ticketingevents/TicketingEventFeatures', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Listing
  ticketingeventlisting(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/ticketingevents/TicketingEventListing', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }


  // Ticketing Event Creation
  userticketingeventcreation(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/staffevents/StaffEventCreation', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // USer Ticketing Event Feature
  userticketingeventfeatures(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/ticketingevents/TicketingEventFeatures', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Event Listing
  userticketingeventlisting(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/ticketingevents/TicketingEventListing', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }


  // Event Features
  eventfeatures(value) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/events/EventFeatures', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getTicketsAnalytics() {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetTicketAnalyticsDetails', options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Event Listing
  eventlisting(userID: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetAllTicketingEventsByUserID?userID=' + userID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Staff Listing
  stafflisting(staffID: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetStaffList?userID=' + staffID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getCountryList() {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetCountriesList', options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getStaffList(userID: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetStaffList?userID=' + userID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getEventStaffList(userID: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetStaffListOfEvent?eventid=' + userID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getVenueDetailsbyId(venueId: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetVenueDetailsbyId?venueId=' + venueId, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getVenueDetailsbyName(name: string) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetVenueDetailsbyName?name=' + name, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getCategoryByEventId(Id) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetTicketEventSummaryByEventID?EventID=' + Id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getVenueList() {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetVenueList', options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getArtistList() {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetArtistsList', options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }


  getEventAnalytics(eventID: number) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetEventDetailsByEventId?eventID=' + eventID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getEventGenderAnalytics(eventID: number) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetNumberOfMalesFemales?eventID=' + eventID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getEventTicketSummaryAnalytics(eventID: number) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetTickeySummaryDetailsByEventId?eventId=' + eventID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  GetAgeGroupAnalytics(eventID: number) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetTicketSummaryByAgeGroupEventId?eventId=' + eventID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getEventUsers(eventID: number) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetListOfAttendingUsers?eventID=' + eventID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }


  deleteEvent(id: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + "api/TicketingAPI/DeleteTicketingEventById?eventID=" + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler);
  }
  ExportEvent(id: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + "api/TicketingAPI/ExportCSV?eventId=" + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler);
  }

  ExportEventAdmin(id: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + "api/TicketingAPI/downloadEventCSVFile?Eventid=" + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler);
  }

  deleteStaffMember(id: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + "api/TicketingAPI/DeleteStaffMember?userId=" + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler);
  }


  getEventById(eventId: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetTicketingEventsByIdForEdit?eventId=' + eventId, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getStaffById(userID: string) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetUserProfileForWeb?_UserID=' + userID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  // Ticket Generation  
  ticketgeneration(value) {
    let headers = new Headers();
    headers.append('Content-Type', 'application/json');
    let options = new RequestOptions({ headers: headers });
    return this._http.post(this.myAppUrl + 'api/events/EventListing', value)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }


  // Staff Event Listing
  getcouponlisting(userID) {

    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetCouponsByUserID?UserID=' + userID, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  getCouponById(id) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + 'api/TicketingAPI/GetCouponsByID?Id=' + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler)
  }

  deleteCoupon(id) {
    let headers: Headers = new Headers();
    headers.append('Access-Control-Allow-Origin', '*');
    headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    headers.set('Authorization', "Basic " + btoa("sdsol:sdsol99"))
    let options = new RequestOptions({ headers: headers });
    return this._http.get(this.myAppUrl + "api/TicketingAPI/DeleteCouponById?Id=" + id, options)
      .map((response: Response) => response.json())
      .catch(this.errorHandler);
  }

  // Error Handler
  errorHandler(error: Response) {
    return Observable.throw(error);
  }
}
