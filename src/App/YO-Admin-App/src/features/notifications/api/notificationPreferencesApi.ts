import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationPreferenceViewModel {
  notificationPreferenceId: number;
  userId: number;
  emailEnabled: boolean;
  smsEnabled: boolean;
  pushEnabled: boolean;
  inAppEnabled: boolean;
  quietHoursEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  mutedCategories: string[];
}

export interface NotificationPreferenceFormViewModel {
  notificationPreferenceId: number;
  userId: number;
  emailEnabled: boolean;
  smsEnabled: boolean;
  pushEnabled: boolean;
  inAppEnabled: boolean;
  quietHoursEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  mutedCategories: string[];
}

export const notificationPreferencesApi = createApi({
  reducerPath: 'notificationPreferencesApi',
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
  tagTypes: ['NotificationPreference'],
  endpoints: (builder) => ({
    getPreferenceByUserId: builder.query<NotificationPreferenceViewModel, number>({
      query: (userId) => `/notification-preferences/users/${userId}`,
      providesTags: ['NotificationPreference'],
    }),

    updatePreferenceByUserId: builder.mutation<void, { userId: number; body: NotificationPreferenceFormViewModel }>({
      query: ({ userId, body }) => ({
        url: `/notification-preferences/users/${userId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['NotificationPreference'],
    }),

    // User-facing
    getMyPreferences: builder.query<NotificationPreferenceViewModel, void>({
      query: () => '/notifications/preferences/my',
      providesTags: ['NotificationPreference'],
    }),

    updateMyPreferences: builder.mutation<void, NotificationPreferenceFormViewModel>({
      query: (body) => ({
        url: '/notifications/preferences/my',
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['NotificationPreference'],
    }),
  }),
});

export const {
  useGetPreferenceByUserIdQuery,
  useUpdatePreferenceByUserIdMutation,
  useGetMyPreferencesQuery,
  useUpdateMyPreferencesMutation,
} = notificationPreferencesApi;