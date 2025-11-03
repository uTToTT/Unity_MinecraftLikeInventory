# Unity Minecraft-Like Inventory Prototype

A **Unity prototype** demonstrating a **Minecraft-style inventory system** with stackable items, drag & drop, and split mechanics.

This project is ideal for testing **inventory mechanics**, **stacking logic**, and **UI interactions** in Unity.

## Overview

The system includes:

- ✅ Stackable items (`InventoryStack`)
    
- ✅ Inventory slots (`InventorySlot`)
    
- ✅ Drag-and-drop support with **left/right mouse buttons**
    
- ✅ Splitting stacks evenly between multiple slots
    
- ✅ Manual item addition for testing (e.g., Coal, Iron)
    
- ✅ Fully modular and reusable controller (`InventoryController`)
    

This prototype mimics **inventory behavior similar to Minecraft**, allowing players to:

- Pick up and move items
    
- Merge stacks
    
- Split stacks with right-click
    
- Automatically handle empty stacks

## Core Components

### `InventoryController`

Main class handling:

- Adding/removing items
    
- Stack management and mapping by `ItemID`
    
- Drag-and-drop logic (LMB & RMB)
    
- Stack splitting and combining
    
- Update loop to move selected stacks with the cursor
    

### `InventoryStack`

Represents a **stack of items**:

- Keeps track of quantity
    
- Notifies when the stack is empty (`OnEmpty`)
    
- Handles visual updates for quantity
    
- Maximum stack size defined per `ItemSO`
    

### `InventorySlot`

Represents a **single inventory slot**:

- Highlights on pointer hover
    
- Holds a single stack at a time
    
- Exposes events for pointer enter/exit
    

### `InventoryManager`

Demo script for **testing the inventory**:

- Adds predefined items (Coal, Iron) via UI buttons
    
- Works directly with `InventoryController`

## Features

- Left-click drag to move or merge stacks
    
- Right-click drag to split stacks
    
- Double-click to merge stacks into the selected stack
    
- Automatic handling of full/empty slots
    
- Supports multiple items with unique IDs
