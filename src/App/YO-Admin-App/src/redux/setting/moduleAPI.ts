import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "../../config/apiConfig";
import { API } from "../../config/apiUrls";

export type ModuleActionRequest = { moduleName: string };

export const moduleAPI = createApi({
    reducerPath: "moduleAPI",
    baseQuery: baseQueryWithAuth,
    tagTypes: ["Module"],
    endpoints: (builder) => ({
        getModules: builder.query<any, { pageNo?: number; pageSize?: number; status?: number; query?: string }>({
            query: ({ pageNo = 1, pageSize = 8, status = 1, query = "" }) => ({
                url: API.MODULE.ALL(pageNo, pageSize, String(status), query),
                method: "GET",
            }),
            providesTags: ["Module"],
        }),

        installModule: builder.mutation<any, ModuleActionRequest>({
            query: (data) => ({
                url: API.MODULE.INSTALL,
                method: "POST",
                body: data,
            }),
            invalidatesTags: ["Module"],
        }),

        uninstallModule: builder.mutation<any, ModuleActionRequest>({
            query: (data) => ({
                url: API.MODULE.UNINSTALL,
                method: "POST",
                body: data,
            }),
            invalidatesTags: ["Module"],
        }),
    }),
});

export const { useGetModulesQuery, useInstallModuleMutation, useUninstallModuleMutation } = moduleAPI;
