import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationEventListViewModel {
  notificationEventId: number;
  eventKey: string;
  name: string;
  moduleName: string;
  isActive: boolean;
  rulesCount: number;
  lastTriggeredOn: string | null;
  addedOn: string;
}

export interface NotificationEventListResponse {
  items: NotificationEventListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationEventFormViewModel {
  notificationEventId: number;
  eventKey: string;
  name: string;
  moduleName: string;
  description: string;
  samplePayloadJson: string;
  isActive: boolean;
}

export interface NotificationEventQueryModel {
  page?: number;
  pageSize?: number;
  keyword?: string;
  moduleName?: string;
  isActive?: boolean | null;
}

export const notificationEventsApi = createApi({
  reducerPath: 'notificationEventsApi',
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
  tagTypes: ['NotificationEvent'],
  endpoints: (builder) => ({
    getEvents: builder.query<NotificationEventListResponse, NotificationEventQueryModel>({
      query: (params) => ({
        url: '/notification-events',
        params,
      }),
      providesTags: ['NotificationEvent'],
    }),

    getEventById: builder.query<NotificationEventFormViewModel, number>({
      query: (id) => `/notification-events/${id}`,
      providesTags: ['NotificationEvent'],
    }),

    createEvent: builder.mutation<{ id: number }, NotificationEventFormViewModel>({
      query: (body) => ({
        url: '/notification-events',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['NotificationEvent'],
    }),

    updateEvent: builder.mutation<void, { id: number; body: NotificationEventFormViewModel }>({
      query: ({ id, body }) => ({
        url: `/notification-events/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['NotificationEvent'],
    }),

    deleteEvent: builder.mutation<void, number>({
      query: (id) => ({
        url: `/notification-events/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NotificationEvent'],
    }),

    toggleEventStatus: builder.mutation<void, { id: number; isActive: boolean }>({
      query: ({ id, isActive }) => ({
        url: `/notification-events/${id}/status`,
        method: 'PATCH',
        body: { isActive },
      }),
      invalidatesTags: ['NotificationEvent'],
    }),

    testTriggerEvent: builder.mutation<any, { id: number; payload: any; actorUserId?: number }>({
      query: ({ id, ...body }) => ({
        url: `/notification-events/${id}/test-trigger`,
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useGetEventsQuery,
  useGetEventByIdQuery,
  useCreateEventMutation,
  useUpdateEventMutation,
  useDeleteEventMutation,
  useToggleEventStatusMutation,
  useTestTriggerEventMutation,
} = notificationEventsApi;