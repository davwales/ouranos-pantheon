"use client";

import OuranosStepper from '@/app/components/ouranos_stepper';
import { Role } from '@/gql/graphql';
import { Box } from '@mui/material';
import { useState } from 'react';
import ChatInterface from './components/chat_interface';
import StepContext from './components/step_context';
import StepSelectCharacter from './components/step_select_character';
import ConversationCharacter from './models/conversation_character';

export default function Conversation() {
    const [context, setContext] = useState('');
    const [userCharacter, setUserCharacter] = useState<ConversationCharacter | undefined>();
    const [assistantCharacter, setAssistantCharacter] = useState<ConversationCharacter | undefined>();
    const [setupComplete, setSetupComplete] = useState(false);

    const steps = [
        {
            label: 'Enter Chat Context',
            component: <StepContext context={context} setContext={setContext} />
        },
        {
            label: 'Select Your Character',
            component: <StepSelectCharacter role={Role.User} character={userCharacter} setCharacter={setUserCharacter} />
        },
        {
            label: 'Select Assistant Character',
            component: <StepSelectCharacter role={Role.Assistant} character={assistantCharacter} setCharacter={setAssistantCharacter} />
        }
    ];

    const handleStepperComplete = () => {
        setSetupComplete(true);
    }

    const setupDisplay = (
        <OuranosStepper sx={{ m: "1rem" }} steps={steps} onComplete={handleStepperComplete} />
    );

    const chatDisplay = userCharacter && assistantCharacter ? (
        <ChatInterface context={context} userCharacter={userCharacter} assistantCharacter={assistantCharacter} />
    ) : "Invalid conversation configuration. Please refresh and try again.";

    return (
        <Box sx={{ width: '100%', height: "100%" }}>
            {setupComplete ? chatDisplay : setupDisplay}
        </Box>
    );
}
