"use client";

import { DataGrid, GridModel } from "@/app/components/core/data-display/data_grid";
import RefreshIcon from "@/app/components/core/icons/refresh_icon";
import Button from "@/app/components/core/inputs/button";
import IconButton from "@/app/components/core/inputs/icon_button";
import Box from "@/app/components/core/layout/box";
import { hasPaginationChanged, mapFilter, mapOrder, mapPagination } from "@/app/components/core/utils/graphql_mappers";
import PaginationInfo from "@/app/models/pagination_info";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { plutusColumns } from "@/app/plutus/constants/plutus_columns";
import { PlutusState, usePlutusStore } from "@/app/plutus/constants/plutus_store";
import { GET_MARKET_TRADES } from "@/app/plutus/queries";
import { GetMarketTradesResponse } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function MarketDetail() {
    const router = useRouter();
    const { marketId } = useParams<{ marketId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);
    const [paginationInfo, setPaginationInfo] = useState<PaginationInfo>();
    const [gridModel, setGridModel] = useState<GridModel>({
        sortModel: [{ field: "totalGain", sort: "desc" }],
        paginationModel: { page: 0, pageSize: 10 },
        filterModel: { items: [] }
    });

    const handleBackClicked = () => {
        router.push("/plutus/explorer");
    };

    const handleTimeFrameChange = (seconds: number) => {
        setTimeFrameSeconds(seconds);
    };

    const handleRowClick = (row: GetMarketTradesResponse) => {
        router.push(`/plutus/explorer/${marketId}/${row.symbolId}`);
    };

    const handleGridModelChanged = (model: GridModel) => {
        if (hasPaginationChanged(model.paginationModel, gridModel.paginationModel)) {
            const paginationInfo = mapPagination(model.paginationModel, gridModel.paginationModel, data?.marketTrades?.pageInfo);
            setPaginationInfo(paginationInfo);
        }

        setGridModel(model);
    };

    const handleRefreshClicked = () => {
        reexecute();
    };

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_MARKET_TRADES,
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
            <Box
                styling={{
                    width: "100%",
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    m: "auto"
                }}
            >
                <Box>
                    <Button variant="outlined" onClick={handleBackClicked}>
                        Back
                    </Button>
                </Box>

                <Box
                    styling={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: "large"
                    }}
                >
                    <IconButton disabled={fetching} onClick={handleRefreshClicked}>
                        <RefreshIcon />
                    </IconButton>
                    <TimeFrameSelection
                        onChange={handleTimeFrameChange}
                        seconds={timeFrameSeconds}
                    />
                </Box>
            </Box>

            <DataGrid
                rows={data?.marketTrades?.nodes}
                columns={plutusColumns}
                getRowId={(row: any) => row.symbolId}
                rowCount={data?.marketTrades?.totalCount || 0}
                loading={fetching}
                initialModel={gridModel}
                onGridModelChange={handleGridModelChanged}
                pageSizeOptions={[5, 10, 15, 20, 50]}
                onRowClick={handleRowClick}
                styling={{ mt: 'medium' }}
            />
        </>
    );
}