import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface NotificationRuleRecipientViewModel {
  notificationRuleRecipientId: number;
  recipientType: string;
  recipientValue: string;
  sortOrder: number;
}

export interface NotificationRuleChannelViewModel {
  notificationRuleChannelId: number;
  channel: string;
  notificationTemplateId: number;
  isRequired: boolean;
  sortOrder: number;
}

export interface NotificationRuleListViewModel {
  notificationRuleId: number;
  name: string;
  eventKey: string;
  priority: number;
  channels: string;
  recipients: string;
  isEnabled: boolean;
  addedOn: string;
}

export interface NotificationRuleListResponse {
  items: NotificationRuleListViewModel[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationRuleFormViewModel {
  notificationRuleId: number;
  notificationEventId: number;
  name: string;
  priority: number;
  isEnabled: boolean;
  conditionJson: string;
  delaySeconds: number;
  maxSendPerUserPerDay: number;
  stopProcessingAfterMatch: boolean;
  quietHoursEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  recipients: NotificationRuleRecipientViewModel[];
  channels: NotificationRuleChannelViewModel[];
}

export interface NotificationRuleQueryModel {
  page?: number;
  pageSize?: number;
  eventKey?: string;
  isEnabled?: boolean | null;
}

export interface ConditionEvaluationRequest {
  conditionJson: string;
  payloadJson: string;
}

export interface ConditionEvaluationResponse {
  matched: boolean;
}

export interface RecipientPreviewRequest {
  eventKey: string;
  actorUserId: number;
  tenantId?: number;
  recipients: NotificationRuleRecipientViewModel[];
}

export interface RecipientUserViewModel {
  userId: number;
  fullName: string;
  email: string;
  phoneNumber: string;
}

export interface RecipientPreviewResponse {
  totalRecipients: number;
  recipients: RecipientUserViewModel[];
}

export const notificationRulesApi = createApi({
  reducerPath: 'notificationRulesApi',
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
  tagTypes: ['NotificationRule'],
  endpoints: (builder) => ({
    getRules: builder.query<NotificationRuleListResponse, NotificationRuleQueryModel>({
      query: (params) => ({
        url: '/notification-rules',
        params,
      }),
      providesTags: ['NotificationRule'],
    }),

    getRuleById: builder.query<NotificationRuleFormViewModel, number>({
      query: (id) => `/notification-rules/${id}`,
      providesTags: ['NotificationRule'],
    }),

    createRule: builder.mutation<{ id: number }, NotificationRuleFormViewModel>({
      query: (body) => ({
        url: '/notification-rules',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['NotificationRule'],
    }),

    updateRule: builder.mutation<void, { id: number; body: NotificationRuleFormViewModel }>({
      query: ({ id, body }) => ({
        url: `/notification-rules/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['NotificationRule'],
    }),

    deleteRule: builder.mutation<void, number>({
      query: (id) => ({
        url: `/notification-rules/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NotificationRule'],
    }),

    toggleRuleStatus: builder.mutation<void, { id: number; isEnabled: boolean }>({
      query: ({ id, isEnabled }) => ({
        url: `/notification-rules/${id}/status`,
        method: 'PATCH',
        body: { isEnabled },
      }),
      invalidatesTags: ['NotificationRule'],
    }),

    cloneRule: builder.mutation<{ id: number }, number>({
      query: (id) => ({
        url: `/notification-rules/${id}/clone`,
        method: 'POST',
      }),
      invalidatesTags: ['NotificationRule'],
    }),

    evaluateCondition: builder.mutation<ConditionEvaluationResponse, ConditionEvaluationRequest>({
      query: (body) => ({
        url: '/notification-rules/evaluate-condition',
        method: 'POST',
        body,
      }),
    }),

    previewRecipients: builder.mutation<RecipientPreviewResponse, RecipientPreviewRequest>({
      query: (body) => ({
        url: '/notification-rules/preview-recipients',
        method: 'POST',
        body,
      }),
    }),

    testRule: builder.mutation<any, { id: number; payload: any; actorUserId?: number }>({
      query: ({ id, ...body }) => ({
        url: `/notification-rules/${id}/test`,
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useGetRulesQuery,
  useGetRuleByIdQuery,
  useCreateRuleMutation,
  useUpdateRuleMutation,
  useDeleteRuleMutation,
  useToggleRuleStatusMutation,
  useCloneRuleMutation,
  useEvaluateConditionMutation,
  usePreviewRecipientsMutation,
  useTestRuleMutation,
} = notificationRulesApi;