import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface AppEventOutboxListViewModel {
  appEventOutboxId: number;
  eventKey: string;
  eventName: string;
  moduleName: string;
  status: string;
  priority: number;
  retryCount: number;
  maxRetryCount: number;
  nextRetryOn: string | null;
  addedOn: string;
  lockedBy: string;
}

export interface AppEventOutboxListResponse {
  items: AppEventOutboxListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AppEventOutboxQueryModel {
  page?: number;
  pageSize?: number;
  eventKey?: string;
  status?: string;
}

export interface AppEventOutboxDetailViewModel {
  appEventOutboxId: number;
  eventKey: string;
  eventName: string;
  moduleName: string;
  tenantId?: number;
  actorUserId?: number;
  correlationId: string;
  idempotencyKey: string;
  payloadJson: string;
  headersJson: string;
  status: string;
  priority: number;
  retryCount: number;
  maxRetryCount: number;
  nextRetryOn?: string;
  lockedBy: string;
  lockedOn?: string;
  processedOn?: string;
  errorMessage: string;
  addedOn: string;
}

export const appEventOutboxApi = createApi({
  reducerPath: 'appEventOutboxApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/admin',
    prepareHeaders: (headers) => {
      const token = localStorage.getItem('access_token');
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['AppEventOutbox'],
  endpoints: (builder) => ({
    getOutbox: builder.query<AppEventOutboxListResponse, AppEventOutboxQueryModel>({
      query: (params) => ({
        url: '/app-event-outbox',
        params,
      }),
      providesTags: ['AppEventOutbox'],
    }),

    getOutboxById: builder.query<AppEventOutboxDetailViewModel, number>({
      query: (id) => `/app-event-outbox/${id}`,
      providesTags: ['AppEventOutbox'],
    }),

    retryOutbox: builder.mutation<any, number>({
      query: (id) => ({
        url: `/app-event-outbox/${id}/retry`,
        method: 'POST',
      }),
      invalidatesTags: ['AppEventOutbox'],
    }),

    deadLetterOutbox: builder.mutation<any, number>({
      query: (id) => ({
        url: `/app-event-outbox/${id}/dead-letter`,
        method: 'POST',
      }),
      invalidatesTags: ['AppEventOutbox'],
    }),

    markCompletedOutbox: builder.mutation<any, number>({
      query: (id) => ({
        url: `/app-event-outbox/${id}/mark-completed`,
        method: 'POST',
      }),
      invalidatesTags: ['AppEventOutbox'],
    }),
  }),
});

export const {
  useGetOutboxQuery,
  useGetOutboxByIdQuery,
  useRetryOutboxMutation,
  useDeadLetterOutboxMutation,
  useMarkCompletedOutboxMutation,
} = appEventOutboxApi;