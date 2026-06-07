import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationSendLogListViewModel {
  notificationSendLogId: number;
  addedOn: string;
  eventKey: string;
  ruleName: string;
  templateKey: string;
  userId: number;
  userName: string;
  channel: string;
  receiver: string;
  status: string;
  provider: string;
  errorMessage: string;
}

export interface NotificationSendLogListResponse {
  items: NotificationSendLogListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationSendLogDetailViewModel {
  notificationSendLogId: number;
  addedOn: string;
  eventKey: string;
  ruleName: string;
  templateKey: string;
  userId: number;
  userName: string;
  channel: string;
  receiver: string;
  status: string;
  provider: string;
  errorMessage: string;
  payloadJson: string;
  renderedSubject: string;
  renderedBody: string;
  retryCount: number;
  sentOn: string | null;
}

export interface NotificationSendLogQueryModel {
  page?: number;
  pageSize?: number;
  eventKey?: string;
  channel?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export const notificationLogsApi = createApi({
  reducerPath: 'notificationLogsApi',
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
  tagTypes: ['NotificationSendLog'],
  endpoints: (builder) => ({
    getLogs: builder.query<NotificationSendLogListResponse, NotificationSendLogQueryModel>({
      query: (params) => ({
        url: '/notification-logs',
        params,
      }),
      providesTags: ['NotificationSendLog'],
    }),

    getLogById: builder.query<NotificationSendLogDetailViewModel, number>({
      query: (id) => `/notification-logs/${id}`,
      providesTags: ['NotificationSendLog'],
    }),

    retryLog: builder.mutation<any, number>({
      query: (id) => ({
        url: `/notification-logs/${id}/retry`,
        method: 'POST',
      }),
      invalidatesTags: ['NotificationSendLog'],
    }),
  }),
});

export const {
  useGetLogsQuery,
  useGetLogByIdQuery,
  useRetryLogMutation,
} = notificationLogsApi;