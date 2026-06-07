import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface RecentEventViewModel {
  eventKey: string;
  name: string;
  triggerCount: number;
  lastTriggeredOn: string;
}

export interface RecentFailedSendViewModel {
  eventKey: string;
  channel: string;
  receiver: string;
  errorMessage: string;
  addedOn: string;
}

export interface TopTriggeredEventViewModel {
  eventKey: string;
  name: string;
  triggerCount: number;
}

export interface NotificationDashboardViewModel {
  totalEvents: number;
  activeRules: number;
  pendingOutbox: number;
  failedNotifications: number;
  emailsSentToday: number;
  inAppSentToday: number;
  deadLetterCount: number;
  recentEvents: RecentEventViewModel[];
  recentFailedSends: RecentFailedSendViewModel[];
  topTriggeredEvents: TopTriggeredEventViewModel[];
}

export const notificationDashboardApi = createApi({
  reducerPath: 'notificationDashboardApi',
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
  tagTypes: ['NotificationDashboard'],
  endpoints: (builder) => ({
    getDashboard: builder.query<NotificationDashboardViewModel, void>({
      query: () => '/notifications/dashboard',
      providesTags: ['NotificationDashboard'],
    }),
  }),
});

export const {
  useGetDashboardQuery,
} = notificationDashboardApi;