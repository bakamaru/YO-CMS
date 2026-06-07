import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface ManualSendDirectRequest {
  channels: string[];
  recipientType: string;
  recipientValue: string;
  subject: string;
  message: string;
  url: string;
  severity: string;
  sendNow: boolean;
  scheduledOn?: string;
}

export interface ManualTriggerEventRequest {
  eventKey: string;
  actorUserId: number;
  tenantId?: number;
  payloadJson: string;
}

export interface ManualSendRecipientViewModel {
  recipientType: string;
  recipientValue: string;
}

export interface ManualSendTemplateRequest {
  notificationTemplateId: number;
  channels: string[];
  recipients: ManualSendRecipientViewModel[];
  payloadJson: string;
}

export const notificationManualSendApi = createApi({
  reducerPath: 'notificationManualSendApi',
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
  tagTypes: ['NotificationSend'],
  endpoints: (builder) => ({
    manualSend: builder.mutation<any, ManualSendDirectRequest>({
      query: (body) => ({
        url: '/notifications/manual-send',
        method: 'POST',
        body,
      }),
    }),

    triggerEvent: builder.mutation<any, ManualTriggerEventRequest>({
      query: (body) => ({
        url: '/notifications/trigger-event',
        method: 'POST',
        body,
      }),
    }),

    sendTemplate: builder.mutation<any, ManualSendTemplateRequest>({
      query: (body) => ({
        url: '/notifications/send-template',
        method: 'POST',
        body,
      }),
    }),

    sendTest: builder.mutation<any, { channel: string }>({
      query: (body) => ({
        url: '/notifications/send-test',
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useManualSendMutation,
  useTriggerEventMutation,
  useSendTemplateMutation,
  useSendTestMutation,
} = notificationManualSendApi;