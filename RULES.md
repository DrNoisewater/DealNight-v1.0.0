# Rules and edition choices

## Edition and sources

The deck uses the **classic 110-card inventory documented in Hasbro B0965 (2014)**, with US property names. This includes **four reference cards and 106 shuffled gameplay cards**. Reference cards are represented by the in-game guide and catalog entries, not dealt into hands. This is not a later edition with a different inventory.

Primary sources checked on September 10, 2026:

- Hasbro B0965 product/instructions: https://instructions.hasbro.com/en-us/instruction/monopoly-deal-card-game
- Hasbro B0965 card-detail and inventory PDF: https://instructions.hasbro.com/api/download/B0965_en-us_monopoly-deal-card-game.pdf
- The licensed Cartamundi/Shuffle classic guide: https://shufflecardgames.com/resources/manuals/en_gb/MonopolyDeal_GameGuide_EN.pdf
- Hasbro E3113 product page, used as a cross-check of player count and objective: https://instructions.hasbro.com/en-us/instruction/monopoly-deal-card-game-instructions

The Hasbro B0965 downloadable page covers card details and inventory; the licensed guide supplies additional turn and payment detail. These sources do not resolve every interaction identically. The explicit choices below are part of this implementation, not claims that Hasbro has issued an exhaustive ruling on each case. The licensed Shuffle guide differs on end-turn discard placement; this implementation follows the requested classic discard-pile/reshuffle behavior, not that guide's bottom-of-draw-pile variant.

No source artwork is included. Card names and mathematical game data are implemented as functional identifiers.

## Exact inventory

| Type | Count |
| --- | ---: |
| Natural property | 28 |
| Property wildcard | 11 |
| Rent | 13 |
| Other actions, including buildings | 34 |
| Money | 20 |
| Quick reference | 4 |
| Total physical inventory | 110 |
| Shuffled gameplay inventory | 106 |

All amounts below are in millions. Property count also gives the full-set size.

| Group | Natural count | Card value | Rent at 1 / 2 / 3 / 4 properties |
| --- | ---: | ---: | --- |
| Brown | 2 | 1 | 1 / 2 |
| Light blue | 3 | 1 | 1 / 2 / 3 |
| Pink | 3 | 2 | 1 / 2 / 4 |
| Orange | 3 | 2 | 1 / 3 / 5 |
| Red | 3 | 3 | 2 / 3 / 6 |
| Yellow | 3 | 3 | 2 / 4 / 6 |
| Green | 3 | 4 | 2 / 4 / 7 |
| Dark blue | 2 | 4 | 3 / 8 |
| Railroad | 4 | 2 | 1 / 2 / 3 / 4 |
| Utility | 2 | 2 | 1 / 2 |

| Wildcard colors | Count | Value |
| --- | ---: | ---: |
| Brown / Light blue | 1 | 1 |
| Light blue / Railroad | 1 | 4 |
| Pink / Orange | 2 | 2 |
| Red / Yellow | 2 | 3 |
| Dark blue / Green | 1 | 4 |
| Green / Railroad | 1 | 4 |
| Railroad / Utility | 1 | 2 |
| Any color | 2 | 0 |

Paired rent cards: two each for Brown/Light blue, Pink/Orange, Red/Yellow, Dark blue/Green and Railroad/Utility. Each has bank value 1. Three any-color rent cards have bank value 3.

| Action | Count | Bank value | Effect |
| --- | ---: | ---: | --- |
| Pass Go | 10 | 1 | Draw two |
| Debt Collector | 3 | 3 | One opponent pays 5 |
| It's My Birthday | 3 | 2 | Every opponent pays 2 |
| Sly Deal | 3 | 3 | Take one unprotected property |
| Forced Deal | 3 | 3 | Swap one property each |
| Deal Breaker | 2 | 5 | Take a complete set with buildings |
| Just Say No | 3 | 4 | Cancel an action against you or its counter |
| House | 3 | 3 | Add 3 to a full street set's rent |
| Hotel | 2 | 4 | Add 4; requires a house |
| Double the Rent | 2 | 1 | Multiply rent by two |

Money quantities: 1×6, 2×5, 3×3, 4×3, 5×2, 10×1; 20 cards totaling 57M. `Assets/Resources/CardInventory.tsv` lists every card ID individually. `Catalog.Create(true)` includes reference cards; the default excludes them.

## Turns, banks and payment

Deal five to each player; player zero starts. At the beginning of each turn draw two, or five if the hand was empty. Play up to three cards and end early if desired. Each banked card, property, building or ordinary action costs one play. Excess hand cards are individually discarded at turn end until seven remain. Running out of draw cards shuffles the discard pile; if both are empty, draw only what is available.

Money and eligible actions can be banked. Following the wording printed in B0965, **valued two-color wildcards may also be banked**. This differs from versions/groups that prohibit banking all properties. Ordinary properties and zero-value universal wildcards cannot be banked. Once banked, a card permanently acts as money even after payment to someone else.

Debtors choose valued assets from their bank and property area; never their hand. They may break complete sets. A payment must reach the debt or exhaust all valued assets. No change is returned, and redundant IDs or another player's assets are rejected before mutation. A player with no valued assets confirms a zero payment. Universal wildcards cannot be paid.

Money stays in the receiving bank. Properties enter the receiver's property area in their current color, joining an available matching group or starting another. Transferred wildcards retain their assignment until their new owner's turn. Buildings paid individually enter the receiving bank and become money.

## Property groups and actions

Three complete groups in **different colors** win. Duplicate-color sets count once. Groups have their printed capacity; excess cards start separate groups. Set IDs distinguish same-color collections. On your turn outside reactions/payments, consolidate cards into matching same-color groups without consuming plays. **User-requested rule in v1.3:** explicitly rotating an already-laid wildcard to a different allowed color consumes one play. This is a custom rule replacing the earlier free-rotation interpretation. No table card returns to a hand.

A group must contain at least one valued property/wildcard to charge rent or count as complete. Universal wildcards may be laid down alone but cannot collect rent alone, form an all-universal winning group, or pay debts.

Sly Deal cannot take from a complete set. Forced Deal's opponent-side target cannot belong to a complete set; the initiator may voluntarily give a property from their own complete set. Deal Breaker transfers an entire chosen complete group, retaining buildings and group identity. There is no mandatory automatic merger of two complete same-color groups.

Paired rent charges every opponent for one matching group. Any-color rent charges one selected opponent for one chosen group. Double the Rent must accompany rent; one double gives 2×, two give 4×. Rent plus two doubles consumes all three plays. This implementation permits doubles on either rent type; the shorter wording about “standard” rent in the guides is interpreted as a rent action, not as a ban on the multicolor card.

Houses require a complete street set with no house. Hotels additionally require a house and no hotel. Never build on railroads/utilities. One house and one hotel can coexist, giving +7 rent in total.

## Explicit edge-case interpretations

- **Reaction timing and cost:** a target gets an untimed response window before resolution. Just Say No cards are reactive and cost no ordinary plays, including counters on the active player's turn. Source/target alternate until one passes. An odd number of Nos cancels; an even number restores. Neither the guide's “any time” wording nor every older edition clarifies own-turn counter budgeting. This implementation deliberately uses the free-reaction interpretation.
- **Multiplayer effects:** resolve opponents clockwise in separate windows. A No cancels only that opponent's obligation, not the entire Birthday or paired rent action. The source may counter that cancellation. Other players cannot interject on someone else's behalf.
- **Rent modifiers:** a No addresses the whole modified rent request for that target. There is no separate partial cancellation of just one double. Played modifiers remain spent even if the request is cancelled.
- **Victory:** check the active player after an atomic action and after every pending effect has settled. Off-turn gains cannot declare victory until the new owner's turn. This follows the licensed classic guide's turn-based claim timing. No victory interrupts an unresolved Birthday, payment or No chain.
- **Broken building groups:** if a payment or rearrangement breaks a built set, buildings stay attached but their rent bonuses are dormant. A hotel without a house is also dormant. They can be paid as valued assets or moved on their owner's turn to a legally eligible full set. Restoring the original set reactivates valid bonuses. This is an explicit resolution for an underspecified case; buildings are not silently destroyed or returned to hand.
- **Building payment:** an individually transferred building becomes bank money; Deal Breaker is the exception that moves buildings as buildings with their complete group.
- **Empty targets:** debt/rent/Birthday may target an assetless opponent; they may respond then pay zero. Steals and building actions need a legal target to appear as Play choices; otherwise banking may remain available.
- **Property reorganization:** separately laid groups are not forcibly merged. Consolidation is available through validated Move commands on the owner's turn. No rearrangement interrupts someone else's action.
- **No draw and no discard:** there is no invented sudden-death rule. Turns remain legal. The simulator's safety limit is a test failure threshold only.

These decisions are fixed rather than selectable house-rule toggles. Reactions, ownership, play budgets and placement are validated centrally for both human and computer players.

## v1.3 rotation and placement clarification

Receiving/stealing a wildcard never charges an additional placement/rotation play. It enters your property area in exactly its prior color, even if the other color would be better. The action used to steal still has its own normal cost. Playing a wildcard from hand retains the normal one-card play cost; choosing its initial color is not an additional action. Changing a laid wildcard to another color during your own playing phase costs one play and is unavailable at zero plays. Moving that card between groups of the same color stays free. Human and AI use the same validation.

Automatic destination selection never changes a wildcard's color. The UI auto-joins a sole eligible group or a unique set-completing group; competing choices remain explicit. Atomic receipts prefer a near-complete matching group, then dormant buildings, then stable set order. Complete sets are never split or merged to make room.
