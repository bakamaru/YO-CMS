import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationInboxListViewModel {
  notificationInboxId: number;
  userId: number;
  userName: string;
  title: string;
  message: string;
  eventKey: string;
  channel: string;
  severity: string;
  isRead: boolean;
  addedOn: string;
}

export interface NotificationInboxListResponse {
  items: NotificationInboxListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationInboxQueryModel {
  page?: number;
  pageSize?: number;
  userId?: number | null;
  isRead?: boolean | null;
  eventKey?: string;
}

export interface MyNotificationViewModel {
  notificationInboxId: number;
  title: string;
  message: string;
  eventKey: string;
  channel: string;
  severity: string;
  url: string;
  isRead: boolean;
  addedOn: string;
}

export interface MyNotificationListResponse {
  items: MyNotificationViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UnreadCountViewModel {
  unreadCount: number;
}

export const notificationInboxApi = createApi({
  reducerPath: 'notificationInboxApi',
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
  tagTypes: ['NotificationInbox'],
  endpoints: (builder) => ({
    getInbox: builder.query<NotificationInboxListResponse, NotificationInboxQueryModel>({
      query: (params) => ({
        url: '/notification-inbox',
        params,
      }),
      providesTags: ['NotificationInbox'],
    }),

    // User-facing endpoints
    getMyNotifications: builder.query<MyNotificationListResponse, { page?: number; pageSize?: number }>({
      query: (params) => ({
        url: '/notifications/my',
        params,
      }),
      providesTags: ['NotificationInbox'],
    }),

    getUnreadCount: builder.query<UnreadCountViewModel, void>({
      query: () => '/notifications/my/unread-count',
      providesTags: ['NotificationInbox'],
    }),

    markRead: builder.mutation<void, number>({
      query: (id) => ({
        url: `/notifications/${id}/read`,
        method: 'PATCH',
      }),
      invalidatesTags: ['NotificationInbox'],
    }),

    markAllRead: builder.mutation<void, void>({
      query: () => ({
        url: '/notifications/read-all',
        method: 'PATCH',
      }),
      invalidatesTags: ['NotificationInbox'],
    }),

    deleteNotification: builder.mutation<void, number>({
      query: (id) => ({
        url: `/notifications/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NotificationInbox'],
    }),
  }),
});

export const {
  useGetInboxQuery,
  useGetMyNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkReadMutation,
  useMarkAllReadMutation,
  useDeleteNotificationMutation,
} = notificationInboxApi;