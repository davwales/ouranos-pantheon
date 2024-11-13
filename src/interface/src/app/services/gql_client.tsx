"use client";

import { UrqlProvider } from "@urql/next";
import { registerUrql } from "@urql/next/rsc";
import { useMemo } from "react";
import { cacheExchange, createClient, fetchExchange, ssrExchange } from "urql";

const gql_url = `${process.env.NEXT_PUBLIC_API_HOST}`;

const makeClient = () => {
    return createClient({
        url: gql_url,
        exchanges: [cacheExchange, fetchExchange],
    });
};

export const { getClient } = registerUrql(makeClient);

export function TalosProvider({ children }: React.PropsWithChildren) {
    const [client, ssr] = useMemo(() => {
        const ssr = ssrExchange({
            isClient: typeof window !== 'undefined',
        });
        const client = createClient({
            url: gql_url,
            exchanges: [cacheExchange, ssr, fetchExchange],
            requestPolicy: "cache-and-network"
        });

        return [client, ssr];
    }, []);

    return (
        <UrqlProvider client={client} ssr={ssr}>
            {children}
        </UrqlProvider>
    );
}