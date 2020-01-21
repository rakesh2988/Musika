export class User {
  UserName: string;
  password: string;
  Email: string;
  RecordStatus: string;
  UserID: number;
}

export class Ticket {
  EventId: number;
  Currency: string;
  CountryId: number;
  RefundPolicy: string;
  ServiceFee: string;
  Tax: string;
  lstTicketData: Array<TicketData>;
}

export class TicketData {
  EventId: number;
  //TicketType: string;
  Quantity: number;
  Price: string;
  TicketCategory: string;
  PackageStartDate: any;
  PackageEndDate: any;
  
}

export class event {
  EventID: number;
  EventTitle: string;
  EventLocation: string;
  VenueName: string;
  Address1: string;
  Address2: string;
  City: string;
  State: string;
  ZipCode: string;
  StartDate: string;
  StartTime: string;
  EndDate: string;
  EndTime: string;
  EventImage: string;
  EventDescription: string;
  OrganizerName: string;
  OrganizerDescription: string;

  ListingPrivacy: string;
  EventType: string;
  EventTopic: string;
}
