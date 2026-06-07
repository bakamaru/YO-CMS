import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationTemplateListViewModel {
  notificationTemplateId: number;
  templateKey: string;
  name: string;
  channel: string;
  languageCode: string;
  isDefault: boolean;
  isActive: boolean;
  version: number;
  addedOn: string;
}

export interface NotificationTemplateListResponse {
  items: NotificationTemplateListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationTemplateFormViewModel {
  notificationTemplateId: number;
  templateKey: string;
  name: string;
  channel: string;
  languageCode: string;
  subjectTemplate: string;
  bodyTemplate: string;
  samplePayloadJson: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface NotificationTemplateQueryModel {
  page?: number;
  pageSize?: number;
  channel?: string;
  keyword?: string;
  isActive?: boolean | null;
}

export interface NotificationTemplatePreviewRequest {
  subjectTemplate: string;
  bodyTemplate: string;
  payloadJson: string;
}

export interface NotificationTemplatePreviewResponse {
  subject: string;
  body: string;
  missingVariables: string[];
  usedVariables: string[];
}

export interface NotificationTemplateValidateRequest {
  subjectTemplate: string;
  bodyTemplate: string;
  availableVariablesJson: string;
}

export interface NotificationTemplateValidateResponse {
  isValid: boolean;
  usedVariables: string[];
  missingVariables: string[];
  availableVariables: string[];
}

export const notificationTemplatesApi = createApi({
  reducerPath: 'notificationTemplatesApi',
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
  tagTypes: ['NotificationTemplate'],
  endpoints: (builder) => ({
    getTemplates: builder.query<NotificationTemplateListResponse, NotificationTemplateQueryModel>({
      query: (params) => ({
        url: '/notification-templates',
        params,
      }),
      providesTags: ['NotificationTemplate'],
    }),

    getTemplatesByChannel: builder.query<NotificationTemplateListViewModel[], string>({
      query: (channel) => `/notification-templates/by-channel/${channel}`,
      providesTags: ['NotificationTemplate'],
    }),

    getTemplateById: builder.query<NotificationTemplateFormViewModel, number>({
      query: (id) => `/notification-templates/${id}`,
      providesTags: ['NotificationTemplate'],
    }),

    createTemplate: builder.mutation<{ id: number }, NotificationTemplateFormViewModel>({
      query: (body) => ({
        url: '/notification-templates',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['NotificationTemplate'],
    }),

    updateTemplate: builder.mutation<void, { id: number; body: NotificationTemplateFormViewModel }>({
      query: ({ id, body }) => ({
        url: `/notification-templates/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['NotificationTemplate'],
    }),

    deleteTemplate: builder.mutation<void, number>({
      query: (id) => ({
        url: `/notification-templates/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NotificationTemplate'],
    }),

    previewTemplate: builder.mutation<NotificationTemplatePreviewResponse, NotificationTemplatePreviewRequest>({
      query: (body) => ({
        url: '/notification-templates/preview',
        method: 'POST',
        body,
      }),
    }),

    validateTemplate: builder.mutation<NotificationTemplateValidateResponse, NotificationTemplateValidateRequest>({
      query: (body) => ({
        url: '/notification-templates/validate',
        method: 'POST',
        body,
      }),
    }),

    cloneTemplate: builder.mutation<{ id: number }, number>({
      query: (id) => ({
        url: `/notification-templates/${id}/clone`,
        method: 'POST',
      }),
      invalidatesTags: ['NotificationTemplate'],
    }),

    testSendTemplate: builder.mutation<any, { id: number; payload: any }>({
      query: ({ id, ...body }) => ({
        url: `/notification-templates/${id}/test-send`,
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useGetTemplatesQuery,
  useGetTemplatesByChannelQuery,
  useGetTemplateByIdQuery,
  useCreateTemplateMutation,
  useUpdateTemplateMutation,
  useDeleteTemplateMutation,
  usePreviewTemplateMutation,
  useValidateTemplateMutation,
  useCloneTemplateMutation,
  useTestSendTemplateMutation,
} = notificationTemplatesApi;