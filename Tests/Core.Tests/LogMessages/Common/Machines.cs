﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Coyote.Actors;

namespace Microsoft.Coyote.Core.Tests.LogMessages
{
    internal class Configure : Event
    {
        public TaskCompletionSource<bool> Tcs;

        public Configure(TaskCompletionSource<bool> tcs)
        {
            this.Tcs = tcs;
        }
    }

    internal class E : Event
    {
        public ActorId Id;

        public E(ActorId id)
        {
            this.Id = id;
        }
    }

    internal class Unit : Event
    {
    }

    internal class M : StateMachine
    {
        private TaskCompletionSource<bool> Tcs;

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(E), nameof(Act))]
        private class Init : State
        {
        }

        private async Task InitOnEntry()
        {
            this.Tcs = (this.ReceivedEvent as Configure).Tcs;
            var nTcs = new TaskCompletionSource<bool>();
            var n = this.CreateStateMachine(typeof(N), new Configure(nTcs));
            await nTcs.Task;
            this.SendEvent(n, new E(this.Id));
        }

        private void Act()
        {
            this.Tcs.SetResult(true);
        }
    }

    internal class N : StateMachine
    {
        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventGotoState(typeof(E), typeof(Act))]
        private class Init : State
        {
        }

        private void InitOnEntry()
        {
            var tcs = (this.ReceivedEvent as Configure).Tcs;
            tcs.SetResult(true);
        }

        [OnEntry(nameof(ActOnEntry))]
        private class Act : State
        {
        }

        private void ActOnEntry()
        {
            ActorId m = (this.ReceivedEvent as E).Id;
            this.SendEvent(m, new E(this.Id));
        }
    }
}
