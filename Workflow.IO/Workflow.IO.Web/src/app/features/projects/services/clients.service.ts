import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHttpService } from '../../../core/services/api-http.service';

export interface ClientResponse {
  clientId: string;
  name: string;
  industry: string;
  contactPerson: string;
  email: string;
  keywords: string;
}

export interface CreateClientRequest {
  name: string;
  industry: string;
  contactPerson: string;
  email: string;
  keywords: string;
}

export interface UpdateClientRequest {
  name: string;
  industry: string;
  contactPerson: string;
  email: string;
  keywords: string;
}

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly api = inject(ApiHttpService);

  getClients(): Observable<ClientResponse[]> {
    return this.api.getList<ClientResponse>('/clients');
  }

  getClientById(clientId: string): Observable<ClientResponse> {
    return this.api.get<ClientResponse>(`/clients/${clientId}`);
  }

  createClient(request: CreateClientRequest): Observable<ClientResponse> {
    return this.api.post<ClientResponse>('/clients', request);
  }

  updateClient(clientId: string, request: UpdateClientRequest): Observable<ClientResponse> {
    return this.api.put<ClientResponse>(`/clients/${clientId}`, request);
  }

  deleteClient(clientId: string): Observable<void> {
    return this.api.delete(`/clients/${clientId}`);
  }
}
