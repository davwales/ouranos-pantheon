import { GridFilterModel, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";

export default interface OuranosGridModel {
    sortModel: GridSortModel,
    filterModel: GridFilterModel,
    paginationModel: GridPaginationModel
};