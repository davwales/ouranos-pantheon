"use client";

import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { GetMarketTradesResponse } from "@/gql/graphql";
import OuranosPaginationInfo from "@/app/models/ouranos_pagination_info";
import OuranosGridModel from "@/app/models/ouranos_grid_model";
import { mapFilter, mapOrder, mapPagination } from "@/app/utilities/graphql_mappers";
import { getMarketTradesQuery } from "../queries";
import { plutusColumns } from "../constants/plutus_columns";
import { OuranosDataGrid } from "@/app/components/ouranos_data_grid";
import { Box, Button } from "@mui/material";
import TimeFrameSelection from "../components/time_frame_selection";
import { PlutusState, usePlutusStore } from "../constants/plutus_store";
import { useQuery } from "@urql/next";

export default function MarketDetail() {
    const router = useRouter();
    const { marketId } = useParams<{ marketId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);
    const [paginationInfo, setPaginationInfo] = useState<OuranosPaginationInfo>();
    const [gridModel, setGridModel] = useState<OuranosGridModel>({
        sortModel: [{ field: "totalGain", sort: "desc" }],
        paginationModel: { page: 0, pageSize: 10 },
        filterModel: { items: [] }
    });

    const handleBackClicked = () => {
        router.push("/plutus");
    };

    const handleTimeFrameChange = (seconds: number) => {
        setTimeFrameSeconds(seconds);
    };

    const handleRowClick = (row: GetMarketTradesResponse) => {
        router.push(`/plutus/${marketId}/${row.symbolId}`);
    };

    const handleGridModelChanged = (model: OuranosGridModel) => {
        const paginationInfo = mapPagination(model.paginationModel, gridModel.paginationModel, data?.marketTrades?.pageInfo);
        setPaginationInfo(paginationInfo);
        setGridModel(model);
    };

    const [{ data, fetching }] = useQuery({
        query: getMarketTradesQuery,
        variables: {
            input: {
                marketId: marketId,
                seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined
            },
            where: mapFilter(gridModel.filterModel, plutusColumns),
            order: mapOrder(gridModel.sortModel),
            after: paginationInfo?.after,
            first: paginationInfo?.first,
            before: paginationInfo?.before,
            last: paginationInfo?.last
        }
    });

    return (
        <>
            <Box sx={{ width: "100%", m: "auto" }}>
                <Button variant="outlined" onClick={handleBackClicked}>Back</Button>
                <TimeFrameSelection onChange={handleTimeFrameChange} seconds={timeFrameSeconds} sx={{ float: "right" }} />
            </Box>
            <OuranosDataGrid
                rows={data?.marketTrades?.nodes}
                columns={plutusColumns}
                getRowId={(row: any) => row.symbolId}
                rowCount={data?.marketTrades?.totalCount || 0}
                loading={fetching}
                initialModel={gridModel}
                onGridModelChange={handleGridModelChanged}
                pageSizeOptions={[5, 10, 15, 20]}
                onRowClick={handleRowClick}
                sx={{ mt: "1rem" }}
            />
        </>
    );
}