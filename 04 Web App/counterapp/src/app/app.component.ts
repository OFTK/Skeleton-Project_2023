import { Component } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { map, switchMap } from "rxjs/operators";

interface SignalRConnection {
  url: string;
  accessToken: string;
}

interface Counter {
  id: number;
  count: number;
}

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.less"]
})
export class AppComponent {
  private readonly httpOptions = { headers: new HttpHeaders({
     "Content-Type": "application/json" ,
     "Access-Control-Allow-Origin": "*",
     "Access-Control-Allow-Methods": "POST, GET",
     "Access-Control-Allow-Headers": "Content-Type"
    }) };
  private readonly negotiateUrl = "https://skeletonfunctionapp.azurewebsites.net/api/negotiate";
  private readonly getCounterUrl = "https://skeletonfunctionapp.azurewebsites.net/api/getcounter";
  private readonly updateCounterUrl = "https://skeletonfunctionapp.azurewebsites.net/api/updatecounter";
  // private readonly negotiateUrl = "http://localhost:7071/api/negotiate";
  // private readonly getCounterUrl = "http://localhost:7071/api/getcounter";
  // private readonly updateCounterUrl = "http://localhost:7071/api/updatecounter";

  private readonly counterId = 1;

  private hubConnection: signalR.HubConnection;
  private counter: number = 0;

  constructor(private readonly http: HttpClient) {
    const negotiateBody = { UserId: "SomeUser" };

    this.http
      .post<SignalRConnection>(this.negotiateUrl, JSON.stringify(negotiateBody), this.httpOptions)
      .pipe(
        map(connectionDetails =>
          new signalR.HubConnectionBuilder().withUrl(`${connectionDetails.url}`, { accessTokenFactory: () => connectionDetails.accessToken }).build()
        )
      )
      .subscribe(hub => {
        this.hubConnection = hub;
        hub.on("CounterUpdate", data => {
          console.log(data);
          this.counter = data.Count;
        });
        hub.start();
      });

    this.http.get<Counter>(this.getCounterUrl).subscribe(cloudCounter => {
      console.log(cloudCounter);
      this.counter = +cloudCounter.count;
    });
  }

  public increaseCounter(): void {
    const body = { Id: this.counterId, counter: this.counter +=1 };

    this.http
      .post(this.updateCounterUrl, body, this.httpOptions)
      .toPromise()
      .catch(e => console.log(e));
  }
}
