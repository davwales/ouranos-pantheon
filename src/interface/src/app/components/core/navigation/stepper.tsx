import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Stepper as MuiStepper, Step, StepLabel } from "@mui/material";

export interface StepDefinition {
    label: string;
    component: React.ReactNode;
};

interface StepperProps {
    steps: StepDefinition[];
    activeStep: number;
    styling?: StyleProps;
}

export function Stepper(props: StepperProps) {
    return (
        <MuiStepper activeStep={props.activeStep} sx={props.styling && convertToSx(props.styling)}>
            {props.steps.map((step, index) => (
                <Step key={index}>
                    <StepLabel>{step.label}</StepLabel>
                </Step>
            ))}
        </MuiStepper>
    );
}
