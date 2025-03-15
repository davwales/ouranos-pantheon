"use client";

import { DataGrid, GridColDef, GridModel } from "@/app/components/core/data-display/data_grid";
import RefreshIcon from "@/app/components/core/icons/refresh_icon";
import Button from "@/app/components/core/inputs/button";
import IconButton from "@/app/components/core/inputs/icon_button";
import Box from "@/app/components/core/layout/box";
import { hasPaginationChanged, mapFilter, mapOrder, mapPagination } from "@/app/components/core/utils/graphql_mappers";
import { abbreviateNumber } from "@/app/components/utils/pretty_number";
import PaginationInfo from "@/app/models/pagination_info";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/constants/plutus_store";
import { GET_RECIPE_TRADES } from "@/app/plutus/queries";
import { GetRecipeTradesResponse } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function RecentMarketTrades() {
    const router = useRouter();
    const { marketId } = useParams<{ marketId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);
    const [paginationInfo, setPaginationInfo] = useState<PaginationInfo>();
    const [gridModel, setGridModel] = useState<GridModel>({
        sortModel: [{ field: "averageMargin", sort: "desc" }],
        paginationModel: { page: 0, pageSize: 10 },
        filterModel: { items: [] }
    });

    const handleBackClicked = () => {
        router.push("/plutus/recipes");
    };

    const handleGridModelChanged = (model: GridModel) => {
        if (hasPaginationChanged(model.paginationModel, gridModel.paginationModel)) {
            const paginationInfo = mapPagination(model.paginationModel, gridModel.paginationModel, data?.recipeTrades?.pageInfo);
            setPaginationInfo(paginationInfo);
        }

        setGridModel(model);
    };

    const columns: GridColDef[] = [
        {
            field: "recipeName",
            headerName: "Name",
            flex: 1
        },
        {
            field: "latestBuyPrice",
            headerName: "Latest Buy Price",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "latestSellPrice",
            headerName: "Latest Sell Price",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "latestMargin",
            headerName: "Latest Margin",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "averageBuyPrice",
            headerName: "Average Buy Price",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "averageSellPrice",
            headerName: "Average Sell Price",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "averageMargin",
            headerName: "Average Margin",
            flex: 1,
            valueFormatter: x => abbreviateNumber(x),
        }
    ]

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_RECIPE_TRADES,
        variables: {
            input: {
                marketId: marketId,
                seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined
            },
            where: mapFilter(gridModel.filterModel, columns),
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
                    <IconButton disabled={fetching} onClick={reexecute}>
                        <RefreshIcon />
                    </IconButton>
                    <TimeFrameSelection
                        onChange={setTimeFrameSeconds}
                        seconds={timeFrameSeconds}
                    />
                </Box>
            </Box>

            <DataGrid
                rows={data?.recipeTrades?.nodes}
                columns={columns}
                getRowId={(row: GetRecipeTradesResponse) => row.recipeId}
                rowCount={data?.recipeTrades?.totalCount || 0}
                loading={fetching}
                initialModel={gridModel}
                onGridModelChange={handleGridModelChanged}
                pageSizeOptions={[5, 10, 15, 20, 50]}
                styling={{ mt: 'medium' }}
            />
        </>
    );
}